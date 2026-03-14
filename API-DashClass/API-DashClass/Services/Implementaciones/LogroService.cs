using API_DashClass.Data;
using API_DashClass.Models.Entities;
using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;
using API_DashClass.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_DashClass.Services.Implementaciones
{
    public class LogroService : ILogroService
    {
        private readonly AppDbContext _context;

        public LogroService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea un nuevo logro en un curso
        /// </summary>
        public async Task<LogroResponse> CrearLogroAsync(CrearLogroRequest request)
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
                throw new InvalidOperationException("Solo los profesores pueden crear logros");
            }

            var nuevoLogro = new Logros
            {
                IdCurso = request.IdCurso,
                IdUsuario = request.IdUsuario,
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                UrlIcono = request.Icono,
                Criterios = request.CondicionDesbloqueo,
                Estatus = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Logros.Add(nuevoLogro);
            await _context.SaveChangesAsync();

            return await MapearLogroAResponseAsync(nuevoLogro);
        }

        /// <summary>
        /// Obtiene todos los logros de un curso
        /// </summary>
        public async Task<List<LogroResponse>> ObtenerLogrosCursoAsync(int idCurso)
        {
            var logros = await _context.Logros
                .Where(l => l.IdCurso == idCurso)
                .OrderByDescending(l => l.Estatus)
                .ThenByDescending(l => l.FechaCreacion)
                .ToListAsync();

            var responses = new List<LogroResponse>();
            foreach (var logro in logros)
            {
                responses.Add(await MapearLogroAResponseAsync(logro));
            }

            return responses;
        }

        /// <summary>
        /// Obtiene todos los logros activos de un curso
        /// </summary>
        public async Task<List<LogroResponse>> ObtenerLogrosActivosAsync(int idCurso)
        {
            var logros = await _context.Logros
                .Where(l => l.IdCurso == idCurso && l.Estatus == true)
                .OrderByDescending(l => l.FechaCreacion)
                .ToListAsync();

            var responses = new List<LogroResponse>();
            foreach (var logro in logros)
            {
                responses.Add(await MapearLogroAResponseAsync(logro));
            }

            return responses;
        }

        /// <summary>
        /// Obtiene los detalles de un logro específico
        /// </summary>
        public async Task<LogroResponse?> ObtenerLogroPorIdAsync(int idLogro)
        {
            var logro = await _context.Logros
                .FirstOrDefaultAsync(l => l.IdLogro == idLogro);

            if (logro == null)
            {
                return null;
            }

            return await MapearLogroAResponseAsync(logro);
        }

        /// <summary>
        /// Actualiza un logro existente
        /// </summary>
        public async Task<LogroResponse> ActualizarLogroAsync(int idLogro, ActualizarLogroRequest request)
        {
            var logro = await _context.Logros
                .FirstOrDefaultAsync(l => l.IdLogro == idLogro);

            if (logro == null)
            {
                throw new InvalidOperationException("Logro no encontrado");
            }

            logro.Nombre = request.Nombre;
            logro.Descripcion = request.Descripcion;
            logro.UrlIcono = request.Icono;
            logro.Criterios = request.CondicionDesbloqueo;
            logro.Estatus = request.Estatus;

            await _context.SaveChangesAsync();

            return await MapearLogroAResponseAsync(logro);
        }

        /// <summary>
        /// Activa o desactiva un logro
        /// </summary>
        public async Task<bool> CambiarEstatusLogroAsync(int idLogro, bool estatus)
        {
            var logro = await _context.Logros
                .FirstOrDefaultAsync(l => l.IdLogro == idLogro);

            if (logro == null)
            {
                return false;
            }

            logro.Estatus = estatus;
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Desbloquea un logro para un usuario y otorga puntos
        /// </summary>
        public async Task<LogroUsuarioResponse> DesbloquearLogroAsync(DesbloquearLogroRequest request)
        {
            // Obtener logro
            var logro = await _context.Logros
                .FirstOrDefaultAsync(l => l.IdLogro == request.IdLogro);

            if (logro == null)
            {
                throw new InvalidOperationException("Logro no encontrado");
            }

            if (!logro.Estatus)
            {
                throw new InvalidOperationException("Este logro no está activo");
            }

            // Verificar si el usuario ya tiene el logro
            var yaDesbloqueado = await _context.LogrosUsuario
                .AnyAsync(lu => lu.IdLogro == request.IdLogro && lu.IdUsuario == request.IdUsuario);

            if (yaDesbloqueado)
            {
                throw new InvalidOperationException("El usuario ya tiene este logro desbloqueado");
            }

            // Crear registro de logro desbloqueado
            var logroUsuario = new LogrosUsuario
            {
                IdLogro = request.IdLogro,
                IdUsuario = request.IdUsuario,
                FechaDesbloqueo = DateTime.UtcNow
            };

            _context.LogrosUsuario.Add(logroUsuario);
            await _context.SaveChangesAsync();

            return await MapearLogroUsuarioAResponseAsync(logroUsuario);
        }

        /// <summary>
        /// Obtiene todos los logros desbloqueados de un usuario en un curso
        /// </summary>
        public async Task<List<LogroUsuarioResponse>> ObtenerLogrosUsuarioAsync(int idUsuario, int idCurso)
        {
            var logrosUsuario = await _context.LogrosUsuario
                .Include(lu => lu.Logro)
                .Where(lu => lu.IdUsuario == idUsuario && lu.Logro!.IdCurso == idCurso)
                .OrderByDescending(lu => lu.FechaDesbloqueo)
                .ToListAsync();

            var responses = new List<LogroUsuarioResponse>();
            foreach (var logroUsuario in logrosUsuario)
            {
                responses.Add(await MapearLogroUsuarioAResponseAsync(logroUsuario));
            }

            return responses;
        }

        /// <summary>
        /// Obtiene el progreso de logros de un usuario (X de Y logros)
        /// </summary>
        public async Task<ProgresoLogrosResponse> ObtenerProgresoLogrosAsync(int idUsuario, int idCurso)
        {
            // Obtener total de logros activos en el curso
            var logrosActivos = await _context.Logros
                .Where(l => l.IdCurso == idCurso && l.Estatus == true)
                .ToListAsync();

            var logrosTotales = logrosActivos.Count;

            // Obtener logros desbloqueados del usuario en el curso
            var logrosDesbloqueados = await _context.LogrosUsuario
                .Include(lu => lu.Logro)
                .Where(lu => lu.IdUsuario == idUsuario &&
                            lu.Logro!.IdCurso == idCurso &&
                            lu.Logro.Estatus == true)
                .ToListAsync();

            var cantidadDesbloqueados = logrosDesbloqueados.Count;

            // Calcular porcentaje
            var porcentaje = logrosTotales > 0
                ? Math.Round((decimal)cantidadDesbloqueados / logrosTotales * 100, 2)
                : 0;

            return new ProgresoLogrosResponse
            {
                IdUsuario = idUsuario,
                IdCurso = idCurso,
                LogrosDesbloqueados = cantidadDesbloqueados,
                LogrosTotales = logrosTotales,
                PorcentajeCompletado = porcentaje
            };
        }

        /// <summary>
        /// Verifica si un usuario tiene un logro específico
        /// </summary>
        public async Task<bool> UsuarioTieneLogroAsync(int idUsuario, int idLogro)
        {
            return await _context.LogrosUsuario
                .AnyAsync(lu => lu.IdUsuario == idUsuario && lu.IdLogro == idLogro);
        }

        // ========================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ========================================

        /// <summary>
        /// Mapea una entidad Logro a LogroResponse
        /// </summary>
        private async Task<LogroResponse> MapearLogroAResponseAsync(Logros logro)
        {
            // Obtener información del creador
            var creador = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == logro.IdUsuario);

            var nombreCreador = creador != null
                ? $"{creador.Nombre} {creador.Apellidos}"
                : "Usuario desconocido";

            // Contar cuántos usuarios han desbloqueado este logro
            var totalDesbloqueado = await _context.LogrosUsuario
                .CountAsync(lu => lu.IdLogro == logro.IdLogro);

            return new LogroResponse
            {
                IdLogro = logro.IdLogro,
                IdCurso = logro.IdCurso,
                IdUsuario = logro.IdUsuario,
                Nombre = logro.Nombre,
                Descripcion = logro.Descripcion,
                Icono = logro.UrlIcono,
                CondicionDesbloqueo = logro.Criterios,
                Estatus = logro.Estatus,
                FechaCreacion = logro.FechaCreacion,
                NombreCreador = nombreCreador,
                TotalDesbloqueado = totalDesbloqueado
            };
        }

        /// <summary>
        /// Mapea una entidad LogroUsuario a LogroUsuarioResponse
        /// </summary>
        private async Task<LogroUsuarioResponse> MapearLogroUsuarioAResponseAsync(LogrosUsuario logroUsuario)
        {
            // Obtener información del logro (puede estar cargado con Include)
            var logro = logroUsuario.Logro ?? await _context.Logros
                .FirstOrDefaultAsync(l => l.IdLogro == logroUsuario.IdLogro);

            // Obtener información del usuario
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == logroUsuario.IdUsuario);

            var nombreUsuario = usuario != null
                ? $"{usuario.Nombre} {usuario.Apellidos}"
                : "Usuario desconocido";

            return new LogroUsuarioResponse
            {
                IdLogroUsuario = logroUsuario.IdLogroUsuario,
                IdLogro = logroUsuario.IdLogro,
                NombreLogro = logro?.Nombre ?? "Logro desconocido",
                DescripcionLogro = logro?.Descripcion,
                IconoLogro = logro?.UrlIcono,
                IdUsuario = logroUsuario.IdUsuario,
                NombreUsuario = nombreUsuario,
                FechaDesbloqueo = logroUsuario.FechaDesbloqueo
            };
        }
    }
}