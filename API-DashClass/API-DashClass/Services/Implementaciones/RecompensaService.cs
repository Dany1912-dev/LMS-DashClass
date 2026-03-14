using API_DashClass.Data;
using API_DashClass.Models.Entities;
using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;
using API_DashClass.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_DashClass.Services.Implementaciones
{
    public class RecompensaService : IRecompensaService
    {
        private readonly AppDbContext _context;

        public RecompensaService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea una nueva recompensa en un curso
        /// </summary>
        public async Task<RecompensaResponse> CrearRecompensaAsync(CrearRecompensaRequest request)
        {
            // Validar que el curso exista
            var cursoExiste = await _context.Cursos.AnyAsync(c => c.IdCurso == request.IdCurso);
            if (!cursoExiste)
            {
                throw new InvalidOperationException("El curso especificado no existe");
            }

            // Validar que el usuario sea profesor del curso
            var esProfesor = await _context.MiembrosCurso
                .AnyAsync(m => m.IdCurso == request.IdCurso &&
                              m.IdUsuario == request.IdUsuario &&
                              m.Rol == MiembrosCursos.RolMiembro.Profesor);

            if (!esProfesor)
            {
                throw new InvalidOperationException("Solo los profesores pueden crear recompensas");
            }

            var nuevaRecompensa = new Recompensas
            {
                IdCurso = request.IdCurso,
                IdUsuario = request.IdUsuario,
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Costo = request.Costo,
                StockGlobal = request.StockGlobal,        // null = ilimitado
                StockRestante = request.StockGlobal,      // inicialmente igual al global
                Destacado = request.Destacado,
                Estatus = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Recompensas.Add(nuevaRecompensa);
            await _context.SaveChangesAsync();

            return await MapearRecompensaAResponseAsync(nuevaRecompensa);
        }

        /// <summary>
        /// Obtiene todas las recompensas activas de un curso
        /// </summary>
        public async Task<List<RecompensaResponse>> ObtenerRecompensasActivasAsync(int idCurso)
        {
            var recompensas = await _context.Recompensas
                .Where(r => r.IdCurso == idCurso && r.Estatus == true)
                .OrderByDescending(r => r.Destacado)
                .ThenByDescending(r => r.FechaCreacion)
                .ToListAsync();

            var responses = new List<RecompensaResponse>();
            foreach (var recompensa in recompensas)
            {
                responses.Add(await MapearRecompensaAResponseAsync(recompensa));
            }

            return responses;
        }

        /// <summary>
        /// Obtiene todas las recompensas de un curso (incluyendo inactivas)
        /// </summary>
        public async Task<List<RecompensaResponse>> ObtenerTodasRecompensasAsync(int idCurso)
        {
            var recompensas = await _context.Recompensas
                .Where(r => r.IdCurso == idCurso)
                .OrderByDescending(r => r.Estatus)
                .ThenByDescending(r => r.Destacado)
                .ThenByDescending(r => r.FechaCreacion)
                .ToListAsync();

            var responses = new List<RecompensaResponse>();
            foreach (var recompensa in recompensas)
            {
                responses.Add(await MapearRecompensaAResponseAsync(recompensa));
            }

            return responses;
        }

        /// <summary>
        /// Obtiene los detalles de una recompensa específica
        /// </summary>
        public async Task<RecompensaResponse?> ObtenerRecompensaPorIdAsync(int idRecompensa)
        {
            var recompensa = await _context.Recompensas
                .FirstOrDefaultAsync(r => r.IdRecompensa == idRecompensa);

            if (recompensa == null)
            {
                return null;
            }

            return await MapearRecompensaAResponseAsync(recompensa);
        }

        /// <summary>
        /// Actualiza una recompensa existente
        /// </summary>
        public async Task<RecompensaResponse> ActualizarRecompensaAsync(int idRecompensa, ActualizarRecompensaRequest request)
        {
            var recompensa = await _context.Recompensas
                .FirstOrDefaultAsync(r => r.IdRecompensa == idRecompensa);

            if (recompensa == null)
            {
                throw new InvalidOperationException("Recompensa no encontrada");
            }

            // Calcular diferencia de stock para ajustar StockRestante
            if (request.StockGlobal != recompensa.StockGlobal)
            {
                if (request.StockGlobal == null)
                {
                    // Cambiar a ilimitado
                    recompensa.StockRestante = null;
                }
                else if (recompensa.StockGlobal == null)
                {
                    // Cambiar de ilimitado a limitado
                    recompensa.StockRestante = request.StockGlobal;
                }
                else
                {
                    // Ajustar stock restante proporcionalmente
                    var diferencia = request.StockGlobal - recompensa.StockGlobal;
                    recompensa.StockRestante = (recompensa.StockRestante ?? 0) + diferencia;

                    // No permitir stock restante negativo
                    if (recompensa.StockRestante < 0)
                    {
                        recompensa.StockRestante = 0;
                    }
                }
            }

            recompensa.Nombre = request.Nombre;
            recompensa.Descripcion = request.Descripcion;
            recompensa.Costo = request.Costo;
            recompensa.StockGlobal = request.StockGlobal;
            recompensa.Destacado = request.Destacado;
            recompensa.Estatus = request.Estatus;

            await _context.SaveChangesAsync();

            return await MapearRecompensaAResponseAsync(recompensa);
        }

        /// <summary>
        /// Activa o desactiva una recompensa
        /// </summary>
        public async Task<bool> CambiarEstatusRecompensaAsync(int idRecompensa, bool estatus)
        {
            var recompensa = await _context.Recompensas
                .FirstOrDefaultAsync(r => r.IdRecompensa == idRecompensa);

            if (recompensa == null)
            {
                return false;
            }

            recompensa.Estatus = estatus;
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Marca una recompensa como destacada o no
        /// </summary>
        public async Task<bool> CambiarDestacadoRecompensaAsync(int idRecompensa, bool destacado)
        {
            var recompensa = await _context.Recompensas
                .FirstOrDefaultAsync(r => r.IdRecompensa == idRecompensa);

            if (recompensa == null)
            {
                return false;
            }

            recompensa.Destacado = destacado;
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Incrementa el stock de una recompensa
        /// </summary>
        public async Task<bool> AgregarStockAsync(int idRecompensa, int cantidad)
        {
            if (cantidad <= 0)
            {
                throw new InvalidOperationException("La cantidad debe ser mayor a 0");
            }

            var recompensa = await _context.Recompensas
                .FirstOrDefaultAsync(r => r.IdRecompensa == idRecompensa);

            if (recompensa == null)
            {
                return false;
            }

            // Si es ilimitado, no hacer nada
            if (recompensa.StockGlobal == null)
            {
                throw new InvalidOperationException("No se puede agregar stock a una recompensa ilimitada");
            }

            recompensa.StockGlobal += cantidad;
            recompensa.StockRestante = (recompensa.StockRestante ?? 0) + cantidad;

            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Reduce el stock de una recompensa (usado al canjear)
        /// </summary>
        public async Task<bool> ReducirStockAsync(int idRecompensa, int cantidad)
        {
            if (cantidad <= 0)
            {
                throw new InvalidOperationException("La cantidad debe ser mayor a 0");
            }

            var recompensa = await _context.Recompensas
                .FirstOrDefaultAsync(r => r.IdRecompensa == idRecompensa);

            if (recompensa == null)
            {
                return false;
            }

            // Si es ilimitado, no reducir stock
            if (recompensa.StockRestante == null)
            {
                return true; // Éxito, pero sin modificar nada
            }

            if (recompensa.StockRestante < cantidad)
            {
                throw new InvalidOperationException($"Stock insuficiente. Stock restante: {recompensa.StockRestante}");
            }

            recompensa.StockRestante -= cantidad;
            await _context.SaveChangesAsync();

            return true;
        }

        // ========================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ========================================

        /// <summary>
        /// Mapea una entidad Recompensa a RecompensaResponse
        /// </summary>
        private async Task<RecompensaResponse> MapearRecompensaAResponseAsync(Recompensas recompensa)
        {
            // Obtener información del creador
            var creador = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == recompensa.IdUsuario);

            var nombreCreador = creador != null
                ? $"{creador.Nombre} {creador.Apellidos}"
                : "Usuario desconocido";

            // Determinar si es ilimitado
            var esIlimitado = recompensa.StockGlobal == null;

            // Determinar si está disponible para canje
            var disponible = recompensa.Estatus && (esIlimitado || (recompensa.StockRestante ?? 0) > 0);

            return new RecompensaResponse
            {
                IdRecompensa = recompensa.IdRecompensa,
                IdCurso = recompensa.IdCurso,
                IdUsuario = recompensa.IdUsuario,
                Nombre = recompensa.Nombre,
                Descripcion = recompensa.Descripcion,
                Costo = recompensa.Costo,
                StockGlobal = recompensa.StockGlobal,
                StockRestante = recompensa.StockRestante,
                EsIlimitado = esIlimitado,
                Destacado = recompensa.Destacado,
                Estatus = recompensa.Estatus,
                FechaCreacion = recompensa.FechaCreacion,
                NombreCreador = nombreCreador,
                DisponibleParaCanje = disponible
            };
        }
    }
}