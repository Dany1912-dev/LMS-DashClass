using Microsoft.EntityFrameworkCore;
using API_DashClass.Data;
using API_DashClass.Models.Entities;
using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;
using API_DashClass.Services.Interfaces;

namespace API_DashClass.Services.Implementaciones
{
    public class CanjeService : ICanjeService
    {
        private readonly AppDbContext _context;
        private readonly IGamificationService _gamificationService;
        private readonly IRecompensaService _recompensaService;

        public CanjeService(
            AppDbContext context,
            IGamificationService gamificationService,
            IRecompensaService recompensaService)
        {
            _context = context;
            _gamificationService = gamificationService;
            _recompensaService = recompensaService;
        }

        /// <summary>
        /// Canjea una recompensa (gasta puntos del estudiante)
        /// </summary>
        public async Task<CanjeResponse> CanjearRecompensaAsync(CanjearRecompensaRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Obtener recompensa
                var recompensa = await _context.Recompensas
                    .FirstOrDefaultAsync(r => r.IdRecompensa == request.IdRecompensa);

                if (recompensa == null)
                {
                    throw new InvalidOperationException("Recompensa no encontrada");
                }

                if (!recompensa.Estatus)
                {
                    throw new InvalidOperationException("Esta recompensa no está disponible");
                }

                if (recompensa.IdCurso != request.IdCurso)
                {
                    throw new InvalidOperationException("La recompensa no pertenece a este curso");
                }

                // Validar stock (si no es ilimitado)
                if (recompensa.StockRestante != null && recompensa.StockRestante <= 0)
                {
                    throw new InvalidOperationException("Recompensa agotada");
                }

                // Obtener balance del estudiante
                var balance = await _gamificationService.ObtenerBalanceAsync(request.IdUsuario, request.IdCurso);

                if (balance == null || balance.PuntosActuales < recompensa.Costo)
                {
                    throw new InvalidOperationException(
                        $"Puntos insuficientes. Necesitas {recompensa.Costo} puntos, tienes {balance?.PuntosActuales ?? 0}");
                }

                // Reducir stock de recompensa (si no es ilimitado)
                if (recompensa.StockRestante != null)
                {
                    await _recompensaService.ReducirStockAsync(request.IdRecompensa, 1);
                }

                // Generar código único de canje
                var codigoCanje = GenerarCodigoCanje();

                // Crear registro de canje
                var nuevoCanje = new Canjes
                {
                    IdRecompensa = request.IdRecompensa,
                    IdUsuario = request.IdUsuario,
                    PuntosGastados = recompensa.Costo,
                    CodigoCanje = codigoCanje,
                    Estado = Canjes.EstadoCanje.Pendiente,
                    FechaCanje = DateTime.UtcNow,
                    FechaReclamado = null
                };

                _context.Canjes.Add(nuevoCanje);
                await _context.SaveChangesAsync();

                // Registrar transacción de puntos (Gastado)
                var transaccionRequest = new ManualPointsRequest
                {
                    IdUsuario = request.IdUsuario,
                    IdCurso = request.IdCurso,
                    Cantidad = -recompensa.Costo,
                    Descripcion = $"Canje de recompensa: {recompensa.Nombre}"
                };

                await _gamificationService.RegistrarPuntosManualAsync(transaccionRequest);

                await transaction.CommitAsync();

                return await MapearCanjeAResponseAsync(nuevoCanje);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Obtiene los canjes de un estudiante en un curso
        /// </summary>
        public async Task<List<CanjeResponse>> ObtenerCanjesEstudianteAsync(int idUsuario, int idCurso)
        {
            var canjes = await _context.Canjes
                .Include(c => c.Recompensa)
                .Where(c => c.IdUsuario == idUsuario && c.Recompensa!.IdCurso == idCurso)
                .OrderByDescending(c => c.FechaCanje)
                .ToListAsync();

            var responses = new List<CanjeResponse>();
            foreach (var canje in canjes)
            {
                responses.Add(await MapearCanjeAResponseAsync(canje));
            }

            return responses;
        }

        /// <summary>
        /// Obtiene todos los canjes de un curso (para profesor)
        /// </summary>
        public async Task<List<CanjeResponse>> ObtenerCanjesCursoAsync(int idCurso, string? estado = null)
        {
            var query = _context.Canjes
                .Include(c => c.Recompensa)
                .Where(c => c.Recompensa!.IdCurso == idCurso);

            // Filtrar por estado si se proporciona
            if (!string.IsNullOrEmpty(estado))
            {
                if (Enum.TryParse<Canjes.EstadoCanje>(estado, true, out var estadoEnum))
                {
                    query = query.Where(c => c.Estado == estadoEnum);
                }
            }

            var canjes = await query
                .OrderByDescending(c => c.FechaCanje)
                .ToListAsync();

            var responses = new List<CanjeResponse>();
            foreach (var canje in canjes)
            {
                responses.Add(await MapearCanjeAResponseAsync(canje));
            }

            return responses;
        }

        /// <summary>
        /// Obtiene los detalles de un canje específico
        /// </summary>
        public async Task<CanjeResponse?> ObtenerCanjePorIdAsync(int idCanje)
        {
            var canje = await _context.Canjes
                .Include(c => c.Recompensa)
                .FirstOrDefaultAsync(c => c.IdCanje == idCanje);

            if (canje == null)
            {
                return null;
            }

            return await MapearCanjeAResponseAsync(canje);
        }

        /// <summary>
        /// Obtiene un canje por su código
        /// </summary>
        public async Task<CanjeResponse?> ObtenerCanjePorCodigoAsync(string codigoCanje)
        {
            var canje = await _context.Canjes
                .Include(c => c.Recompensa)
                .FirstOrDefaultAsync(c => c.CodigoCanje == codigoCanje);

            if (canje == null)
            {
                return null;
            }

            return await MapearCanjeAResponseAsync(canje);
        }

        /// <summary>
        /// Reclama un canje (profesor valida código)
        /// </summary>
        public async Task<CanjeResponse> ReclamarCanjeAsync(string codigoCanje)
        {
            var canje = await _context.Canjes
                .Include(c => c.Recompensa)
                .FirstOrDefaultAsync(c => c.CodigoCanje == codigoCanje);

            if (canje == null)
            {
                throw new InvalidOperationException("Código de canje no válido");
            }

            if (canje.Estado == Canjes.EstadoCanje.Reclamado)
            {
                throw new InvalidOperationException("Este canje ya fue reclamado");
            }

            if (canje.Estado == Canjes.EstadoCanje.Expirado)
            {
                throw new InvalidOperationException("Este canje ha expirado");
            }

            canje.Estado = Canjes.EstadoCanje.Reclamado;
            canje.FechaReclamado = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await MapearCanjeAResponseAsync(canje);
        }

        /// <summary>
        /// Cancela un canje y devuelve los puntos al estudiante
        /// </summary>
        public async Task<bool> CancelarCanjeAsync(int idCanje, int idUsuarioProfesor)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var canje = await _context.Canjes
                    .Include(c => c.Recompensa)
                    .FirstOrDefaultAsync(c => c.IdCanje == idCanje);

                if (canje == null)
                {
                    return false;
                }

                if (canje.Estado == Canjes.EstadoCanje.Reclamado)
                {
                    throw new InvalidOperationException("No se puede cancelar un canje que ya fue reclamado");
                }

                if (canje.Estado == Canjes.EstadoCanje.Expirado)
                {
                    throw new InvalidOperationException("No se puede cancelar un canje expirado");
                }

                // Validar que el usuario sea profesor del curso
                var esProfesor = await _context.MiembrosCurso
                    .AnyAsync(m => m.IdCurso == canje.Recompensa!.IdCurso &&
                                  m.IdUsuario == idUsuarioProfesor &&
                                  m.Rol == MiembrosCursos.RolMiembro.Profesor);

                if (!esProfesor)
                {
                    throw new InvalidOperationException("Solo los profesores pueden cancelar canjes");
                }

                // Devolver stock a recompensa (si no es ilimitado)
                if (canje.Recompensa!.StockRestante != null)
                {
                    await _recompensaService.AgregarStockAsync(canje.IdRecompensa, 1);
                }

                // Devolver puntos al estudiante
                var transaccionRequest = new ManualPointsRequest
                {
                    IdUsuario = canje.IdUsuario,
                    IdCurso = canje.Recompensa.IdCurso,
                    Cantidad = canje.PuntosGastados,
                    Descripcion = $"Devolución por cancelación de canje: {canje.Recompensa.Nombre}"
                };

                await _gamificationService.RegistrarPuntosManualAsync(transaccionRequest);

                // Marcar canje como expirado
                canje.Estado = Canjes.EstadoCanje.Expirado;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Marca canjes expirados automáticamente
        /// </summary>
        public async Task MarcarCanjesExpiradosAsync()
        {
            // Ejemplo: Canjes pendientes por más de 30 días se marcan como expirados
            var fechaLimite = DateTime.UtcNow.AddDays(-30);

            var canjesPendientes = await _context.Canjes
                .Where(c => c.Estado == Canjes.EstadoCanje.Pendiente && c.FechaCanje < fechaLimite)
                .ToListAsync();

            foreach (var canje in canjesPendientes)
            {
                canje.Estado = Canjes.EstadoCanje.Expirado;
            }

            if (canjesPendientes.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        // ========================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ========================================

        /// <summary>
        /// Mapea una entidad Canje a CanjeResponse
        /// </summary>
        private async Task<CanjeResponse> MapearCanjeAResponseAsync(Canjes canje)
        {
            // Obtener información del estudiante
            var estudiante = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == canje.IdUsuario);

            var nombreEstudiante = estudiante != null
                ? $"{estudiante.Nombre} {estudiante.Apellidos}"
                : "Usuario desconocido";

            // Obtener nombre de recompensa (puede estar ya cargada con Include)
            var recompensa = canje.Recompensa ?? await _context.Recompensas
                .FirstOrDefaultAsync(r => r.IdRecompensa == canje.IdRecompensa);

            var nombreRecompensa = recompensa?.Nombre ?? "Recompensa desconocida";

            // Determinar si puede ser cancelado
            var puedeSerCancelado = canje.Estado == Canjes.EstadoCanje.Pendiente;

            return new CanjeResponse
            {
                IdCanje = canje.IdCanje,
                IdRecompensa = canje.IdRecompensa,
                NombreRecompensa = nombreRecompensa,
                IdUsuario = canje.IdUsuario,
                NombreEstudiante = nombreEstudiante,
                PuntosGastados = canje.PuntosGastados,
                CodigoCanje = canje.CodigoCanje,
                Estado = canje.Estado.ToString(),
                FechaCanje = canje.FechaCanje,
                FechaReclamado = canje.FechaReclamado,
                PuedeSerCancelado = puedeSerCancelado
            };
        }

        /// <summary>
        /// Genera un código único de canje
        /// </summary>
        private string GenerarCodigoCanje()
        {
            // Formato: CNJ-XXXXXX (6 caracteres alfanuméricos)
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var codigo = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"CNJ-{codigo}";
        }
    }
}