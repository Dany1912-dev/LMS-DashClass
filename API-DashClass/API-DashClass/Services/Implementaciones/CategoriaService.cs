using API_DashClass.Data;
using API_DashClass.Models.Entities;
using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;
using API_DashClass.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_DashClass.Services.Implementaciones
{
    public class CategoriaService : ICategoriaService
    {
        private readonly AppDbContext _context;

        public CategoriaService(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // CREAR CATEGORÍA
        // ============================================================

        public async Task<CategoriaResponse> CrearCategoriaAsync(CrearCategoriaRequest request)
        {
            var cursoExiste = await _context.Cursos
                .AnyAsync(c => c.IdCurso == request.IdCurso && c.Estatus == true);

            if (!cursoExiste)
                throw new InvalidOperationException("Curso no encontrado o inactivo");

            // Validar que el peso total no supere 100
            var pesoActual = await _context.CategoriasActividad
                .Where(c => c.IdCurso == request.IdCurso)
                .SumAsync(c => c.Peso);

            if (pesoActual + request.Peso > 100)
                throw new InvalidOperationException(
                    $"El peso total supera el 100%. Peso disponible: {100 - pesoActual:F2}%");

            var nueva = new CategoriasActividad
            {
                IdCurso = request.IdCurso,
                Nombre = request.Nombre.Trim(),
                Peso = request.Peso,
                Descripcion = request.Descripcion?.Trim(),
                FechaCreacion = DateTime.UtcNow
            };

            _context.CategoriasActividad.Add(nueva);
            await _context.SaveChangesAsync();

            return await MapearAResponseAsync(nueva);
        }

        // ============================================================
        // OBTENER POR CURSO
        // ============================================================

        public async Task<List<CategoriaResponse>> ObtenerCategoriasCursoAsync(int idCurso)
        {
            var categorias = await _context.CategoriasActividad
                .Where(c => c.IdCurso == idCurso)
                .OrderBy(c => c.FechaCreacion)
                .ToListAsync();

            var responses = new List<CategoriaResponse>();
            foreach (var c in categorias)
                responses.Add(await MapearAResponseAsync(c));

            return responses;
        }

        // ============================================================
        // OBTENER POR ID
        // ============================================================

        public async Task<CategoriaResponse?> ObtenerCategoriaPorIdAsync(int idCategoria)
        {
            var categoria = await _context.CategoriasActividad
                .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria);

            if (categoria == null) return null;

            return await MapearAResponseAsync(categoria);
        }

        // ============================================================
        // ACTUALIZAR
        // ============================================================

        public async Task<CategoriaResponse> ActualizarCategoriaAsync(
            int idCategoria, ActualizarCategoriaRequest request)
        {
            var categoria = await _context.CategoriasActividad
                .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria)
                ?? throw new InvalidOperationException("Categoría no encontrada");

            // Validar peso total excluyendo la categoría actual
            var pesoActual = await _context.CategoriasActividad
                .Where(c => c.IdCurso == categoria.IdCurso && c.IdCategoria != idCategoria)
                .SumAsync(c => c.Peso);

            if (pesoActual + request.Peso > 100)
                throw new InvalidOperationException(
                    $"El peso total supera el 100%. Peso disponible: {100 - pesoActual:F2}%");

            categoria.Nombre = request.Nombre.Trim();
            categoria.Peso = request.Peso;
            categoria.Descripcion = request.Descripcion?.Trim();

            await _context.SaveChangesAsync();

            return await MapearAResponseAsync(categoria);
        }

        // ============================================================
        // ELIMINAR
        // ============================================================

        public async Task<bool> EliminarCategoriaAsync(int idCategoria)
        {
            var categoria = await _context.CategoriasActividad
                .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria);

            if (categoria == null) return false;

            // Las actividades quedan con IdCategoria = null (ON DELETE SET NULL en BD)
            _context.CategoriasActividad.Remove(categoria);
            await _context.SaveChangesAsync();

            return true;
        }

        // ============================================================
        // CALCULAR CALIFICACIÓN FINAL — UN ESTUDIANTE
        // ============================================================

        public async Task<CalificacionFinalResponse> CalcularCalificacionFinalAsync(
            int idUsuario, int idCurso)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            var categorias = await _context.CategoriasActividad
                .Where(c => c.IdCurso == idCurso)
                .OrderBy(c => c.FechaCreacion)
                .ToListAsync();

            var desglose = new List<DesgloseCategoriaResponse>();
            decimal calificacionFinal = 0;

            foreach (var categoria in categorias)
            {
                // Actividades publicadas de esta categoría en el curso
                var actividades = await _context.Actividades
                    .Where(a => a.IdCurso == idCurso &&
                                a.IdCategoria == categoria.IdCategoria &&
                                a.Estatus == EstatusActividad.Publicado)
                    .ToListAsync();

                int totalActividades = actividades.Count;
                int calificadas = 0;
                decimal sumaPromedios = 0;

                foreach (var actividad in actividades)
                {
                    // Buscar entrega calificada del estudiante
                    var entrega = await _context.Entregas
                        .Where(e => e.IdActividad == actividad.IdActividad &&
                                    e.IdUsuario == idUsuario &&
                                    e.Estado == Entregas.EstadoEntrega.Calificada)
                        .OrderByDescending(e => e.Version)
                        .FirstOrDefaultAsync();

                    if (entrega == null) continue;

                    var calificacion = await _context.Calificaciones
                        .FirstOrDefaultAsync(c => c.IdEntrega == entrega.IdEntrega);

                    if (calificacion == null) continue;

                    // Convertir puntuacion a porcentaje sobre puntos_maximos
                    decimal porcentaje = actividad.PuntosMaximos > 0
                        ? (calificacion.Puntuacion / actividad.PuntosMaximos) * 100
                        : 0;

                    sumaPromedios += porcentaje;
                    calificadas++;
                }

                decimal promedioCategoria = calificadas > 0
                    ? Math.Round(sumaPromedios / calificadas, 2)
                    : 0;

                decimal aporte = Math.Round(promedioCategoria * categoria.Peso / 100, 2);
                calificacionFinal += aporte;

                desglose.Add(new DesgloseCategoriaResponse
                {
                    IdCategoria = categoria.IdCategoria,
                    NombreCategoria = categoria.Nombre,
                    Peso = categoria.Peso,
                    PromedioCategoria = promedioCategoria,
                    AporteAlFinal = aporte,
                    ActividadesCalificadas = calificadas,
                    ActividadesTotales = totalActividades
                });
            }

            var pesoTotal = categorias.Sum(c => c.Peso);

            return new CalificacionFinalResponse
            {
                IdUsuario = idUsuario,
                IdCurso = idCurso,
                NombreEstudiante = $"{usuario.Nombre} {usuario.Apellidos}",
                CalificacionFinal = Math.Round(calificacionFinal, 2),
                PesoTotalConfigurado = pesoTotal,
                Desglose = desglose
            };
        }

        // ============================================================
        // CALCULAR CALIFICACIONES — TODOS LOS ESTUDIANTES DEL CURSO
        // ============================================================

        public async Task<List<CalificacionFinalResponse>> CalcularCalificacionesCursoAsync(int idCurso)
        {
            var estudiantes = await _context.MiembrosCurso
                .Where(m => m.IdCurso == idCurso &&
                            m.Rol == MiembrosCursos.RolMiembro.Estudiante &&
                            m.Estatus == true)
                .Select(m => m.IdUsuario)
                .Distinct()
                .ToListAsync();

            var resultados = new List<CalificacionFinalResponse>();

            foreach (var idUsuario in estudiantes)
            {
                var calificacion = await CalcularCalificacionFinalAsync(idUsuario, idCurso);
                resultados.Add(calificacion);
            }

            return resultados.OrderByDescending(r => r.CalificacionFinal).ToList();
        }

        // ============================================================
        // AUXILIAR PRIVADO
        // ============================================================

        private async Task<CategoriaResponse> MapearAResponseAsync(CategoriasActividad categoria)
        {
            var totalActividades = await _context.Actividades
                .CountAsync(a => a.IdCategoria == categoria.IdCategoria);

            return new CategoriaResponse
            {
                IdCategoria = categoria.IdCategoria,
                IdCurso = categoria.IdCurso,
                Nombre = categoria.Nombre,
                Peso = categoria.Peso,
                Descripcion = categoria.Descripcion,
                FechaCreacion = categoria.FechaCreacion,
                TotalActividades = totalActividades
            };
        }
    }
}