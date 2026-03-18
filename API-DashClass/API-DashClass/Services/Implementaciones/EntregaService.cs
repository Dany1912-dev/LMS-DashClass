using API_DashClass.Data;
using API_DashClass.Events;
using API_DashClass.Models.Entities;
using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;
using API_DashClass.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_DashClass.Services.Implementaciones
{
    public class EntregaService : IEntregaService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly BusEventos _busEventos;

        private static readonly HashSet<string> ExtensionsBloqueadas = new(StringComparer.OrdinalIgnoreCase)
        {
            ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".cab", ".iso"
        };

        public EntregaService(AppDbContext context, IWebHostEnvironment env, BusEventos busEventos)
        {
            _context = context;
            _env = env;
            _busEventos = busEventos;
        }

        // ============================================================
        // MARCAR COMO ENTREGADA (sin archivos)
        // ============================================================

        public async Task<EntregaResponse> MarcarEntregadaAsync(MarcarEntregadaRequest request)
        {
            var actividad = await _context.Actividades
                .FirstOrDefaultAsync(a => a.IdActividad == request.IdActividad)
                ?? throw new InvalidOperationException("Actividad no encontrada");

            if (actividad.Estatus != EstatusActividad.Publicado)
                throw new InvalidOperationException("La actividad no está publicada");

            var esTardia = actividad.FechaLimite.HasValue &&
                           DateTime.UtcNow > actividad.FechaLimite.Value;

            if (esTardia && !actividad.PermiteEntregasTardias)
                throw new InvalidOperationException("La actividad no acepta entregas tardías");

            var version = await ObtenerSiguienteVersionAsync(request.IdActividad, request.IdUsuario);

            var entrega = new Entregas
            {
                IdActividad = request.IdActividad,
                IdUsuario = request.IdUsuario,
                Comentarios = request.Comentarios?.Trim(),
                FechaEntrega = DateTime.UtcNow,
                EsTardia = esTardia,
                Version = version,
                Estado = Entregas.EstadoEntrega.Entregada
            };

            _context.Entregas.Add(entrega);
            await _context.SaveChangesAsync();

            return await MapearEntregaAsync(entrega);
        }

        // ============================================================
        // SUBIR ENTREGA CON ARCHIVOS
        // ============================================================

        public async Task<EntregaResponse> SubirEntregaAsync(SubirEntregaRequest request)
        {
            var actividad = await _context.Actividades
                .FirstOrDefaultAsync(a => a.IdActividad == request.IdActividad)
                ?? throw new InvalidOperationException("Actividad no encontrada");

            if (actividad.Estatus != EstatusActividad.Publicado)
                throw new InvalidOperationException("La actividad no está publicada");

            var esTardia = actividad.FechaLimite.HasValue &&
                           DateTime.UtcNow > actividad.FechaLimite.Value;

            if (esTardia && !actividad.PermiteEntregasTardias)
                throw new InvalidOperationException("La actividad no acepta entregas tardías");

            // Validar archivos
            if (request.Archivos != null)
            {
                foreach (var archivo in request.Archivos)
                {
                    var ext = Path.GetExtension(archivo.FileName);
                    if (ExtensionsBloqueadas.Contains(ext))
                        throw new InvalidOperationException(
                            $"El tipo de archivo '{ext}' no está permitido");
                }
            }

            var version = await ObtenerSiguienteVersionAsync(request.IdActividad, request.IdUsuario);

            var entrega = new Entregas
            {
                IdActividad = request.IdActividad,
                IdUsuario = request.IdUsuario,
                Comentarios = request.Comentarios?.Trim(),
                FechaEntrega = DateTime.UtcNow,
                EsTardia = esTardia,
                Version = version,
                Estado = Entregas.EstadoEntrega.Entregada
            };

            _context.Entregas.Add(entrega);
            await _context.SaveChangesAsync();

            // Guardar archivos
            if (request.Archivos != null && request.Archivos.Count > 0)
            {
                var carpetaEntregas = Path.Combine(_env.ContentRootPath, "uploads", "entregas");
                Directory.CreateDirectory(carpetaEntregas);

                foreach (var archivo in request.Archivos)
                {
                    if (archivo.Length == 0) continue;

                    var nombreUnico = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
                    var rutaCompleta = Path.Combine(carpetaEntregas, nombreUnico);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await archivo.CopyToAsync(stream);
                    }

                    _context.RecursosEntrega.Add(new RecursosEntrega
                    {
                        IdEntrega = entrega.IdEntrega,
                        Tipo = RecursosEntrega.TipoRecurso.Archivo,
                        Nombre = archivo.FileName,
                        UrlArchivo = $"/uploads/entregas/{nombreUnico}",
                        TamanoArchivo = archivo.Length,
                        FechaSubida = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
            }

            return await MapearEntregaAsync(entrega);
        }

        // ============================================================
        // OBTENER ENTREGA DE UN ESTUDIANTE
        // ============================================================

        public async Task<EntregaResponse?> ObtenerEntregaEstudianteAsync(int idActividad, int idUsuario)
        {
            var entrega = await _context.Entregas
                .Where(e => e.IdActividad == idActividad &&
                            e.IdUsuario == idUsuario &&
                            e.Estado != Entregas.EstadoEntrega.Reemplazada)
                .OrderByDescending(e => e.Version)
                .FirstOrDefaultAsync();

            if (entrega == null) return null;
            return await MapearEntregaAsync(entrega);
        }

        // ============================================================
        // OBTENER ENTREGAS DE UNA ACTIVIDAD (para profesor)
        // ============================================================

        public async Task<List<EntregaResponse>> ObtenerEntregasActividadAsync(int idActividad)
        {
            // Solo la entrega más reciente por estudiante
            var entregas = await _context.Entregas
                .Where(e => e.IdActividad == idActividad &&
                            e.Estado != Entregas.EstadoEntrega.Reemplazada)
                .OrderByDescending(e => e.FechaEntrega)
                .ToListAsync();

            var responses = new List<EntregaResponse>();
            foreach (var e in entregas)
                responses.Add(await MapearEntregaAsync(e));

            return responses;
        }

        // ============================================================
        // CALIFICAR ENTREGA
        // ============================================================

        public async Task<CalificacionResponse> CalificarEntregaAsync(
            int idEntrega, CalificarEntregaRequest request)
        {
            var entrega = await _context.Entregas
                .FirstOrDefaultAsync(e => e.IdEntrega == idEntrega)
                ?? throw new InvalidOperationException("Entrega no encontrada");

            // Validar que el usuario sea profesor del curso
            var actividad = await _context.Actividades
                .FirstOrDefaultAsync(a => a.IdActividad == entrega.IdActividad)
                ?? throw new InvalidOperationException("Actividad no encontrada");

            var esProfesor = await _context.MiembrosCurso
                .AnyAsync(m => m.IdCurso == actividad.IdCurso &&
                               m.IdUsuario == request.IdUsuarioProfesor &&
                               m.Rol == MiembrosCursos.RolMiembro.Profesor &&
                               m.Estatus == true);

            if (!esProfesor)
                throw new InvalidOperationException("Solo los profesores pueden calificar");

            // Si ya tiene calificación, actualizarla
            var calificacionExistente = await _context.Calificaciones
                .FirstOrDefaultAsync(c => c.IdEntrega == idEntrega);

            if (calificacionExistente != null)
            {
                calificacionExistente.Puntuacion = request.Puntuacion;
                calificacionExistente.Retroalimentacion = request.Retroalimentacion?.Trim();
                calificacionExistente.FechaCalificacion = DateTime.UtcNow;
                calificacionExistente.IdUsuario = request.IdUsuarioProfesor;
            }
            else
            {
                var nuevaCalificacion = new Calificaciones
                {
                    IdEntrega = idEntrega,
                    Puntuacion = request.Puntuacion,
                    Retroalimentacion = request.Retroalimentacion?.Trim(),
                    IdUsuario = request.IdUsuarioProfesor,
                    FechaCalificacion = DateTime.UtcNow
                };
                _context.Calificaciones.Add(nuevaCalificacion);

                // Actualizar estado de la entrega
                entrega.Estado = Entregas.EstadoEntrega.Calificada;
            }

            await _context.SaveChangesAsync();

            var calificacion = await _context.Calificaciones
                .FirstOrDefaultAsync(c => c.IdEntrega == idEntrega);

            // Disparar evento para otorgar puntos de gamificación
            await _busEventos.PublicarAsync(new CalificacionCreadaEvento
            {
                IdEntrega = idEntrega,
                IdActividad = entrega.IdActividad,
                IdUsuario = entrega.IdUsuario,
                IdCurso = actividad.IdCurso,
                Puntuacion = request.Puntuacion,
                PuntosMaximos = actividad.PuntosMaximos,
                PuntosGamificacionMaximos = actividad.PuntosGamificacionMaximos
            });

            return await MapearCalificacionAsync(calificacion!);
        }

        // ============================================================
        // OBTENER CALIFICACIÓN
        // ============================================================

        public async Task<CalificacionResponse?> ObtenerCalificacionAsync(int idEntrega)
        {
            var calificacion = await _context.Calificaciones
                .FirstOrDefaultAsync(c => c.IdEntrega == idEntrega);

            if (calificacion == null) return null;
            return await MapearCalificacionAsync(calificacion);
        }

        // ============================================================
        // PRIVADOS
        // ============================================================

        private async Task<int> ObtenerSiguienteVersionAsync(int idActividad, int idUsuario)
        {
            // Marcar entregas anteriores como Reemplazadas
            var entregasAnteriores = await _context.Entregas
                .Where(e => e.IdActividad == idActividad &&
                            e.IdUsuario == idUsuario &&
                            e.Estado == Entregas.EstadoEntrega.Entregada)
                .ToListAsync();

            foreach (var e in entregasAnteriores)
                e.Estado = Entregas.EstadoEntrega.Reemplazada;

            var ultimaVersion = await _context.Entregas
                .Where(e => e.IdActividad == idActividad && e.IdUsuario == idUsuario)
                .MaxAsync(e => (int?)e.Version) ?? 0;

            return ultimaVersion + 1;
        }

        private async Task<EntregaResponse> MapearEntregaAsync(Entregas entrega)
        {
            var estudiante = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == entrega.IdUsuario);

            var actividad = await _context.Actividades
                .FirstOrDefaultAsync(a => a.IdActividad == entrega.IdActividad);

            var recursos = await _context.RecursosEntrega
                .Where(r => r.IdEntrega == entrega.IdEntrega)
                .ToListAsync();

            var calificacion = await _context.Calificaciones
                .FirstOrDefaultAsync(c => c.IdEntrega == entrega.IdEntrega);

            return new EntregaResponse
            {
                IdEntrega = entrega.IdEntrega,
                IdActividad = entrega.IdActividad,
                TituloActividad = actividad?.Titulo ?? "",
                IdUsuario = entrega.IdUsuario,
                NombreEstudiante = estudiante != null
                    ? $"{estudiante.Nombre} {estudiante.Apellidos}"
                    : "Desconocido",
                FotoPerfilUrl = estudiante?.FotoPerfilUrl,
                Comentarios = entrega.Comentarios,
                FechaEntrega = entrega.FechaEntrega,
                EsTardia = entrega.EsTardia,
                Version = entrega.Version,
                Estado = entrega.Estado.ToString(),
                Recursos = recursos.Select(r => new RecursoEntregaResponse
                {
                    IdRecurso = r.IdRecurso,
                    Tipo = r.Tipo.ToString(),
                    Nombre = r.Nombre,
                    UrlArchivo = r.UrlArchivo,
                    TamanoArchivo = r.TamanoArchivo,
                    UrlExterna = r.UrlExterna,
                    FechaSubida = r.FechaSubida
                }).ToList(),
                Calificacion = calificacion != null
                    ? await MapearCalificacionAsync(calificacion)
                    : null
            };
        }

        private async Task<CalificacionResponse> MapearCalificacionAsync(Calificaciones cal)
        {
            var profesor = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == cal.IdUsuario);

            return new CalificacionResponse
            {
                IdCalificacion = cal.IdCalificacion,
                IdEntrega = cal.IdEntrega,
                Puntuacion = cal.Puntuacion,
                Retroalimentacion = cal.Retroalimentacion,
                IdUsuario = cal.IdUsuario,
                NombreProfesor = profesor != null
                    ? $"{profesor.Nombre} {profesor.Apellidos}"
                    : "Desconocido",
                FechaCalificacion = cal.FechaCalificacion
            };
        }
    }
}