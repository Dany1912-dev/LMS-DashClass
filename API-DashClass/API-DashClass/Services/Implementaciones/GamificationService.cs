using API_DashClass.Data;
using API_DashClass.Models.Entities;
using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;
using API_DashClass.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_DashClass.Services.Implementaciones
{
    public class GamificationService : IGamificationService
    {
        private readonly AppDbContext _context;

        public GamificationService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene el balance actual de puntos de un usuario en un curso
        /// </summary>
        public async Task<BalanceResponse?> ObtenerBalanceAsync(int idUsuario, int idCurso)
        {
            var transacciones = await _context.TransaccionesPuntos
                .Where(t => t.IdUsuario == idUsuario && t.IdCurso == idCurso)
                .ToListAsync();

            if (!transacciones.Any())
            {
                return new BalanceResponse
                {
                    IdUsuario = idUsuario,
                    IdCurso = idCurso,
                    PuntosActuales = 0,
                    TotalGanado = 0,
                    TotalGastado = 0,
                    TotalTransferido = 0,
                    UltimaActualizacion = DateTime.UtcNow
                };
            }

            var ultimaTransaccion = transacciones.OrderByDescending(t => t.FechaCreacion).First();

            var totalGanado = transacciones
                .Where(t => t.Tipo == TransaccionesPuntos.TipoTransaccion.Ganado)
                .Sum(t => t.Cantidad);

            var totalGastado = transacciones
                .Where(t => t.Tipo == TransaccionesPuntos.TipoTransaccion.Gastado)
                .Sum(t => Math.Abs(t.Cantidad));

            var totalTransferido = transacciones
                .Where(t => t.Tipo == TransaccionesPuntos.TipoTransaccion.Transferido)
                .Sum(t => Math.Abs(t.Cantidad));

            return new BalanceResponse
            {
                IdUsuario = idUsuario,
                IdCurso = idCurso,
                PuntosActuales = ultimaTransaccion.BalanceDespues,
                TotalGanado = totalGanado,
                TotalGastado = totalGastado,
                TotalTransferido = totalTransferido,
                UltimaActualizacion = ultimaTransaccion.FechaCreacion
            };
        }

        /// <summary>
        /// Obtiene el historial de transacciones de puntos
        /// </summary>
        public async Task<List<TransaccionResponse>> ObtenerHistorialAsync(int idUsuario, int idCurso, int limite = 50)
        {
            var transacciones = await _context.TransaccionesPuntos
                .Where(t => t.IdUsuario == idUsuario && t.IdCurso == idCurso)
                .OrderByDescending(t => t.FechaCreacion)
                .Take(limite)
                .Select(t => new TransaccionResponse
                {
                    IdTransaccion = t.IdTransaccion,
                    Tipo = t.Tipo.ToString(),
                    Origen = t.Origen.ToString(),
                    Cantidad = t.Cantidad,
                    BalanceDespues = t.BalanceDespues,
                    Descripcion = t.Descripcion,
                    FechaCreacion = t.FechaCreacion
                })
                .ToListAsync();

            return transacciones;
        }

        /// <summary>
        /// Registra puntos manualmente (por el profesor)
        /// </summary>
        public async Task<TransaccionResponse> RegistrarPuntosManualAsync(ManualPointsRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Obtener balance actual
                var balanceActual = await ObtenerBalanceActualInternoAsync(request.IdUsuario, request.IdCurso);

                // Calcular nuevo balance
                var nuevoBalance = balanceActual + request.Cantidad;

                // Validar que no quede negativo
                if (nuevoBalance < 0)
                {
                    throw new InvalidOperationException($"El balance no puede ser negativo. Balance actual: {balanceActual}");
                }

                // Crear transacción
                var nuevaTransaccion = new TransaccionesPuntos
                {
                    IdUsuario = request.IdUsuario,
                    IdCurso = request.IdCurso,
                    Tipo = request.Cantidad > 0
                        ? TransaccionesPuntos.TipoTransaccion.Ganado
                        : TransaccionesPuntos.TipoTransaccion.Gastado,
                    Origen = TransaccionesPuntos.OrigenTransaccion.Manual,
                    Cantidad = request.Cantidad,
                    BalanceDespues = nuevoBalance,
                    Descripcion = request.Descripcion ?? "Puntos otorgados manualmente",
                    FechaCreacion = DateTime.UtcNow
                };

                _context.TransaccionesPuntos.Add(nuevaTransaccion);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new TransaccionResponse
                {
                    IdTransaccion = nuevaTransaccion.IdTransaccion,
                    Tipo = nuevaTransaccion.Tipo.ToString(),
                    Origen = nuevaTransaccion.Origen.ToString(),
                    Cantidad = nuevaTransaccion.Cantidad,
                    BalanceDespues = nuevaTransaccion.BalanceDespues,
                    Descripcion = nuevaTransaccion.Descripcion,
                    FechaCreacion = nuevaTransaccion.FechaCreacion
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Transfiere puntos de un usuario a otro
        /// </summary>
        public async Task<TransferResponse> TransferirPuntosAsync(TransferPointsRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validar que no sea el mismo usuario
                if (request.DesdeIdUsuario == request.HaciaIdUsuario)
                {
                    throw new InvalidOperationException("No puedes transferir puntos a ti mismo");
                }

                // Validar que ambos usuarios existan en el curso
                var usuariosEnCurso = await _context.MiembrosCurso
                    .Where(m => m.IdCurso == request.IdCurso &&
                               (m.IdUsuario == request.DesdeIdUsuario || m.IdUsuario == request.HaciaIdUsuario))
                    .CountAsync();

                if (usuariosEnCurso != 2)
                {
                    throw new InvalidOperationException("Uno o ambos usuarios no pertenecen al curso");
                }

                // Obtener balance del emisor
                var balanceEmisor = await ObtenerBalanceActualInternoAsync(request.DesdeIdUsuario, request.IdCurso);

                // Validar que tenga suficientes puntos
                if (balanceEmisor < request.Cantidad)
                {
                    throw new InvalidOperationException($"Puntos insuficientes. Balance actual: {balanceEmisor}");
                }

                // Obtener balance del receptor
                var balanceReceptor = await ObtenerBalanceActualInternoAsync(request.HaciaIdUsuario, request.IdCurso);

                // Generar código único de transferencia
                var codigoTransferencia = GenerarCodigoTransferencia();

                // Crear registro en transferencias_puntos
                var transferencia = new TransferenciasPuntos
                {
                    DesdeIdUsuario = request.DesdeIdUsuario,
                    HaciaIdUsuario = request.HaciaIdUsuario,
                    IdCurso = request.IdCurso,
                    Cantidad = request.Cantidad,
                    Mensaje = request.Mensaje,
                    Anonima = request.Anonima,
                    CodigoTransferencia = codigoTransferencia,
                    FechaTransferencia = DateTime.UtcNow
                };

                _context.TransferenciasPuntos.Add(transferencia);

                // Crear transacción del emisor (restar puntos)
                var transaccionEmisor = new TransaccionesPuntos
                {
                    IdUsuario = request.DesdeIdUsuario,
                    IdCurso = request.IdCurso,
                    Tipo = TransaccionesPuntos.TipoTransaccion.Transferido,
                    Origen = TransaccionesPuntos.OrigenTransaccion.Social,
                    Cantidad = -request.Cantidad,
                    BalanceDespues = balanceEmisor - request.Cantidad,
                    IdReferencia = transferencia.IdTransferencia,
                    Descripcion = $"Transferencia enviada a usuario {request.HaciaIdUsuario}",
                    FechaCreacion = DateTime.UtcNow
                };

                _context.TransaccionesPuntos.Add(transaccionEmisor);

                // Crear transacción del receptor (sumar puntos)
                var transaccionReceptor = new TransaccionesPuntos
                {
                    IdUsuario = request.HaciaIdUsuario,
                    IdCurso = request.IdCurso,
                    Tipo = TransaccionesPuntos.TipoTransaccion.Ganado,
                    Origen = TransaccionesPuntos.OrigenTransaccion.Social,
                    Cantidad = request.Cantidad,
                    BalanceDespues = balanceReceptor + request.Cantidad,
                    IdReferencia = transferencia.IdTransferencia,
                    Descripcion = request.Anonima
                        ? "Transferencia recibida de usuario anónimo"
                        : $"Transferencia recibida de usuario {request.DesdeIdUsuario}",
                    FechaCreacion = DateTime.UtcNow
                };

                _context.TransaccionesPuntos.Add(transaccionReceptor);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new TransferResponse
                {
                    IdTransferencia = transferencia.IdTransferencia,
                    CodigoTransferencia = transferencia.CodigoTransferencia,
                    DesdeIdUsuario = request.DesdeIdUsuario,
                    HaciaIdUsuario = request.HaciaIdUsuario,
                    Cantidad = request.Cantidad,
                    Anonima = request.Anonima,
                    FechaTransferencia = transferencia.FechaTransferencia,
                    Mensaje = "Transferencia realizada exitosamente"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Obtiene el ranking de estudiantes por puntos
        /// </summary>
        public async Task<List<RankingResponse>> ObtenerRankingAsync(int idCurso, int top = 10)
        {
            // Obtener estudiantes del curso
            var estudiantes = await _context.MiembrosCurso
                .Where(m => m.IdCurso == idCurso && m.Rol == MiembrosCursos.RolMiembro.Estudiante)
                .Select(m => m.IdUsuario)
                .ToListAsync();

            // Obtener última transacción de cada estudiante para obtener su balance
            var balances = new List<(int IdUsuario, int Balance)>();

            foreach (var estudianteId in estudiantes)
            {
                var ultimaTransaccion = await _context.TransaccionesPuntos
                    .Where(t => t.IdUsuario == estudianteId && t.IdCurso == idCurso)
                    .OrderByDescending(t => t.FechaCreacion)
                    .FirstOrDefaultAsync();

                var balance = ultimaTransaccion?.BalanceDespues ?? 0;
                balances.Add((estudianteId, balance));
            }

            // Ordenar por balance y tomar top
            var topEstudiantes = balances
                .OrderByDescending(b => b.Balance)
                .Take(top)
                .ToList();

            // Obtener información de usuarios
            var usuariosIds = topEstudiantes.Select(b => b.IdUsuario).ToList();
            var usuarios = await _context.Usuarios
                .Where(u => usuariosIds.Contains(u.IdUsuario))
                .ToDictionaryAsync(u => u.IdUsuario);

            // Construir ranking
            var ranking = topEstudiantes.Select((item, index) => new RankingResponse
            {
                Posicion = index + 1,
                IdUsuario = item.IdUsuario,
                NombreCompleto = usuarios.ContainsKey(item.IdUsuario)
                    ? $"{usuarios[item.IdUsuario].Nombre} {usuarios[item.IdUsuario].Apellidos}"
                    : "Usuario desconocido",
                FotoPerfilUrl = usuarios.ContainsKey(item.IdUsuario)
                    ? usuarios[item.IdUsuario].FotoPerfilUrl
                    : null,
                TotalPuntos = item.Balance
            }).ToList();

            return ranking;
        }

        /// <summary>
        /// Registra puntos por calificación (llamado por evento)
        /// </summary>
        public async Task RegistrarPuntosPorCalificacionAsync(
            int idEntrega,
            int idUsuario,
            int idCurso,
            decimal puntuacion,
            int puntosMaximos,
            int puntosGamificacionMaximos)
        {
            if (puntosGamificacionMaximos <= 0) return; // No dar puntos si no está configurado

            // Calcular puntos: (calificacion / puntos_maximos) * puntos_gamificacion_maximos
            var puntosObtenidos = (int)Math.Round((puntuacion / puntosMaximos) * puntosGamificacionMaximos);

            var balanceActual = await ObtenerBalanceActualInternoAsync(idUsuario, idCurso);

            var transaccion = new TransaccionesPuntos
            {
                IdUsuario = idUsuario,
                IdCurso = idCurso,
                Tipo = TransaccionesPuntos.TipoTransaccion.Ganado,
                Origen = TransaccionesPuntos.OrigenTransaccion.Calificacion,
                Cantidad = puntosObtenidos,
                BalanceDespues = balanceActual + puntosObtenidos,
                IdReferencia = idEntrega,
                Descripcion = $"Puntos por calificación: {puntuacion}/{puntosMaximos}",
                FechaCreacion = DateTime.UtcNow
            };

            _context.TransaccionesPuntos.Add(transaccion);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Registra puntos por asistencia (llamado por evento)
        /// </summary>
        public async Task RegistrarPuntosPorAsistenciaAsync(int idSesionAsistencia, int idUsuario, int idCurso)
        {
            const int PUNTOS_POR_ASISTENCIA = 5; // Configurable en appsettings

            var balanceActual = await ObtenerBalanceActualInternoAsync(idUsuario, idCurso);

            var transaccion = new TransaccionesPuntos
            {
                IdUsuario = idUsuario,
                IdCurso = idCurso,
                Tipo = TransaccionesPuntos.TipoTransaccion.Ganado,
                Origen = TransaccionesPuntos.OrigenTransaccion.Asistencia,
                Cantidad = PUNTOS_POR_ASISTENCIA,
                BalanceDespues = balanceActual + PUNTOS_POR_ASISTENCIA,
                IdReferencia = idSesionAsistencia,
                Descripcion = "Puntos por asistencia registrada",
                FechaCreacion = DateTime.UtcNow
            };

            _context.TransaccionesPuntos.Add(transaccion);
            await _context.SaveChangesAsync();
        }

        // ========================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ========================================

        /// <summary>
        /// Obtiene el balance actual de un usuario (uso interno)
        /// </summary>
        private async Task<int> ObtenerBalanceActualInternoAsync(int idUsuario, int idCurso)
        {
            var ultimaTransaccion = await _context.TransaccionesPuntos
                .Where(t => t.IdUsuario == idUsuario && t.IdCurso == idCurso)
                .OrderByDescending(t => t.FechaCreacion)
                .FirstOrDefaultAsync();

            return ultimaTransaccion?.BalanceDespues ?? 0;
        }

        /// <summary>
        /// Genera un código único de transferencia
        /// </summary>
        private string GenerarCodigoTransferencia()
        {
            return $"TRF-{Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper()}";
        }
    }
}