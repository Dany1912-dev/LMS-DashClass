using API_DashClass.Data;
using API_DashClass.Models.Entities;
using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;
using API_DashClass.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_DashClass.Services.Implementaciones
{
    public class ActividadService : IActividadService
    {
        private readonly AppDbContext _context;

        public ActividadService(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // CREAR ACTIVIDAD
        // ============================================================

        public async Task<ActividadResponse> CrearActividadAsync(CrearActividadRequest request)
        {
            // Validar curso
            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == request.IdCurso && c.Estatus == true)
                ?? throw new InvalidOperationException("Curso no encontrado o inactivo");

            // Validar que el usuario sea profesor del curso
            var esProfesor = await _context.MiembrosCurso
                .AnyAsync(m => m.IdCurso == request.IdCurso &&
                               m.IdUsuario == request.IdUsuario &&
                               m.Rol == MiembrosCursos.RolMiembro.Profesor &&
                               m.Estatus == true);

            if (!esProfesor)
                throw new InvalidOperationException("Solo los profesores pueden crear actividades");

            // Parsear estatus desde string
            if (!Enum.TryParse<EstatusActividad>(request.Estatus, true, out var estatusEnum))
                estatusEnum = EstatusActividad.Borrador;

            // Si estatus es Publicado, registrar fecha de publicacion
            DateTime? fechaPublicacion = estatusEnum == EstatusActividad.Publicado
                ? DateTime.UtcNow
                : null;

            var nuevaActividad = new Actividades
            {
                IdCurso = request.IdCurso,
                IdUsuario = request.IdUsuario,
                IdCategoria = request.IdCategoria,
                Titulo = request.Titulo.Trim(),
                Descripcion = request.Descripcion?.Trim(),
                PuntosMaximos = request.PuntosMaximos,
                PuntosGamificacionMaximos = request.PuntosGamificacionMaximos,
                FechaLimite = request.FechaLimite,
                PermiteEntregasTardias = request.PermiteEntregasTardias,
                Estatus = estatusEnum,
                FechaPublicacion = fechaPublicacion,
                FechaProgramada = request.FechaProgramada,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Actividades.Add(nuevaActividad);
            await _context.SaveChangesAsync();

            // Obtener grupos del curso
            var gruposCurso = await _context.Grupos
                .Where(g => g.IdCurso == request.IdCurso && g.Estatus == true)
                .ToListAsync();

            // Determinar a qué grupos asignar
            List<int> gruposAAsignar;

            if (request.IdGrupos == null || request.IdGrupos.Count == 0)
            {
                // Asignar a todos los grupos del curso
                gruposAAsignar = gruposCurso.Select(g => g.IdGrupo).ToList();
            }
            else
            {
                // Validar que los grupos pertenecen al curso
                var idsValidos = gruposCurso.Select(g => g.IdGrupo).ToHashSet();
                gruposAAsignar = request.IdGrupos
                    .Where(id => idsValidos.Contains(id))
                    .ToList();

                if (gruposAAsignar.Count == 0)
                    throw new InvalidOperationException("Ninguno de los grupos indicados pertenece al curso");
            }

            // Crear registros en actividades_grupos
            foreach (var idGrupo in gruposAAsignar)
            {
                _context.ActividadesGrupos.Add(new ActividadesGrupos
                {
                    IdActividad = nuevaActividad.IdActividad,
                    IdGrupo = idGrupo
                });
            }

            await _context.SaveChangesAsync();

            return await MapearActividadAResponseAsync(nuevaActividad);
        }

        // ============================================================
        // OBTENER POR CURSO
        // ============================================================

        public async Task<List<ActividadResponse>> ObtenerActividadesCursoAsync(int idCurso, string? estatus = null)
        {
            var query = _context.Actividades
                .Where(a => a.IdCurso == idCurso);

            if (!string.IsNullOrEmpty(estatus) &&
                Enum.TryParse<EstatusActividad>(estatus, true, out var estatusEnum))
            {
                query = query.Where(a => a.Estatus == estatusEnum);
            }

            var actividades = await query
                .OrderByDescending(a => a.FechaCreacion)
                .ToListAsync();

            var responses = new List<ActividadResponse>();
            foreach (var a in actividades)
                responses.Add(await MapearActividadAResponseAsync(a));

            return responses;
        }

        // ============================================================
        // OBTENER POR GRUPO
        // ============================================================

        public async Task<List<ActividadResponse>> ObtenerActividadesGrupoAsync(
            int idCurso, int idGrupo, string? estatus = null)
        {
            // IDs de actividades asignadas al grupo
            var idsActividades = await _context.ActividadesGrupos
                .Where(ag => ag.IdGrupo == idGrupo)
                .Select(ag => ag.IdActividad)
                .ToListAsync();

            var query = _context.Actividades
                .Where(a => a.IdCurso == idCurso && idsActividades.Contains(a.IdActividad));

            if (!string.IsNullOrEmpty(estatus) &&
                Enum.TryParse<EstatusActividad>(estatus, true, out var estatusEnum))
            {
                query = query.Where(a => a.Estatus == estatusEnum);
            }

            var actividades = await query
                .OrderByDescending(a => a.FechaCreacion)
                .ToListAsync();

            var responses = new List<ActividadResponse>();
            foreach (var a in actividades)
                responses.Add(await MapearActividadAResponseAsync(a));

            return responses;
        }

        // ============================================================
        // OBTENER POR ID
        // ============================================================

        public async Task<ActividadResponse?> ObtenerActividadPorIdAsync(int idActividad)
        {
            var actividad = await _context.Actividades
                .FirstOrDefaultAsync(a => a.IdActividad == idActividad);

            if (actividad == null) return null;

            return await MapearActividadAResponseAsync(actividad);
        }

        // ============================================================
        // ACTUALIZAR
        // ============================================================

        public async Task<ActividadResponse> ActualizarActividadAsync(
            int idActividad, ActualizarActividadRequest request)
        {
            var actividad = await _context.Actividades
                .FirstOrDefaultAsync(a => a.IdActividad == idActividad)
                ?? throw new InvalidOperationException("Actividad no encontrada");

            // Parsear estatus desde string
            if (!Enum.TryParse<EstatusActividad>(request.Estatus, true, out var estatusEnum))
                estatusEnum = EstatusActividad.Borrador;

            // Si se está publicando por primera vez, registrar fecha
            if (estatusEnum == EstatusActividad.Publicado &&
                actividad.Estatus != EstatusActividad.Publicado)
            {
                actividad.FechaPublicacion = DateTime.UtcNow;
            }

            actividad.IdCategoria = request.IdCategoria;
            actividad.Titulo = request.Titulo.Trim();
            actividad.Descripcion = request.Descripcion?.Trim();
            actividad.PuntosMaximos = request.PuntosMaximos;
            actividad.PuntosGamificacionMaximos = request.PuntosGamificacionMaximos;
            actividad.FechaLimite = request.FechaLimite;
            actividad.PermiteEntregasTardias = request.PermiteEntregasTardias;
            actividad.Estatus = estatusEnum;
            actividad.FechaProgramada = request.FechaProgramada;

            // Actualizar grupos si se proporcionan
            if (request.IdGrupos != null)
            {
                // Eliminar asignaciones anteriores
                var asignacionesActuales = await _context.ActividadesGrupos
                    .Where(ag => ag.IdActividad == idActividad)
                    .ToListAsync();

                _context.ActividadesGrupos.RemoveRange(asignacionesActuales);

                // Obtener grupos válidos del curso
                var gruposCurso = await _context.Grupos
                    .Where(g => g.IdCurso == actividad.IdCurso && g.Estatus == true)
                    .Select(g => g.IdGrupo)
                    .ToHashSetAsync();

                var gruposAAsignar = request.IdGrupos.Count == 0
                    ? gruposCurso.ToList()
                    : request.IdGrupos.Where(id => gruposCurso.Contains(id)).ToList();

                foreach (var idGrupo in gruposAAsignar)
                {
                    _context.ActividadesGrupos.Add(new ActividadesGrupos
                    {
                        IdActividad = idActividad,
                        IdGrupo = idGrupo
                    });
                }
            }

            await _context.SaveChangesAsync();

            return await MapearActividadAResponseAsync(actividad);
        }

        // ============================================================
        // CAMBIAR ESTATUS
        // ============================================================

        public async Task<bool> CambiarEstatusActividadAsync(int idActividad, string estatus)
        {
            if (!Enum.TryParse<EstatusActividad>(estatus, true, out var estatusEnum))
                throw new InvalidOperationException($"Estatus '{estatus}' no válido");

            var actividad = await _context.Actividades
                .FirstOrDefaultAsync(a => a.IdActividad == idActividad);

            if (actividad == null) return false;

            if (estatusEnum == EstatusActividad.Publicado &&
                actividad.Estatus != EstatusActividad.Publicado)
            {
                actividad.FechaPublicacion = DateTime.UtcNow;
            }

            actividad.Estatus = estatusEnum;
            await _context.SaveChangesAsync();

            return true;
        }

        // ============================================================
        // AUXILIAR PRIVADO
        // ============================================================

        private async Task<ActividadResponse> MapearActividadAResponseAsync(Actividades actividad)
        {
            var profesor = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == actividad.IdUsuario);

            CategoriasActividad? categoria = null;
            if (actividad.IdCategoria.HasValue)
                categoria = await _context.CategoriasActividad
                    .FirstOrDefaultAsync(c => c.IdCategoria == actividad.IdCategoria);

            var asignaciones = await _context.ActividadesGrupos
                .Where(ag => ag.IdActividad == actividad.IdActividad)
                .ToListAsync();

            var gruposCurso = await _context.Grupos
                .Where(g => g.IdCurso == actividad.IdCurso && g.Estatus == true)
                .ToListAsync();

            var gruposAsignados = gruposCurso
                .Where(g => asignaciones.Any(ag => ag.IdGrupo == g.IdGrupo))
                .Select(g => new GrupoActividadResponse
                {
                    IdGrupo = g.IdGrupo,
                    Nombre = g.Nombre
                })
                .ToList();

            var esPorTodos = gruposAsignados.Count == gruposCurso.Count;

            return new ActividadResponse
            {
                IdActividad = actividad.IdActividad,
                IdCurso = actividad.IdCurso,
                IdUsuario = actividad.IdUsuario,
                NombreProfesor = profesor != null
                    ? $"{profesor.Nombre} {profesor.Apellidos}"
                    : "Desconocido",
                IdCategoria = actividad.IdCategoria,
                NombreCategoria = categoria?.Nombre,
                PesoCategoria = categoria?.Peso,
                Titulo = actividad.Titulo,
                Descripcion = actividad.Descripcion,
                PuntosMaximos = actividad.PuntosMaximos,
                PuntosGamificacionMaximos = actividad.PuntosGamificacionMaximos,
                FechaLimite = actividad.FechaLimite,
                PermiteEntregasTardias = actividad.PermiteEntregasTardias,
                Estatus = actividad.Estatus.ToString(),
                FechaPublicacion = actividad.FechaPublicacion,
                FechaProgramada = actividad.FechaProgramada,
                FechaCreacion = actividad.FechaCreacion,
                Grupos = gruposAsignados,
                EsParaTodos = esPorTodos
            };
        }
    }
}