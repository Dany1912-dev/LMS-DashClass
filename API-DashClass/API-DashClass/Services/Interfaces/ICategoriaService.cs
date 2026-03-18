using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;

namespace API_DashClass.Services.Interfaces
{
    public interface ICategoriaService
    {
        /// <summary>Crea una nueva categoría en un curso</summary>
        Task<CategoriaResponse> CrearCategoriaAsync(CrearCategoriaRequest request);

        /// <summary>Obtiene todas las categorías de un curso</summary>
        Task<List<CategoriaResponse>> ObtenerCategoriasCursoAsync(int idCurso);

        /// <summary>Obtiene una categoría por su ID</summary>
        Task<CategoriaResponse?> ObtenerCategoriaPorIdAsync(int idCategoria);

        /// <summary>Actualiza nombre, peso y descripción de una categoría</summary>
        Task<CategoriaResponse> ActualizarCategoriaAsync(int idCategoria, ActualizarCategoriaRequest request);

        /// <summary>Elimina una categoría (las actividades quedan sin categoría)</summary>
        Task<bool> EliminarCategoriaAsync(int idCategoria);

        /// <summary>
        /// Calcula la calificación final ponderada de un estudiante en un curso.
        /// Solo toma en cuenta actividades con categoría asignada y calificadas.
        /// </summary>
        Task<CalificacionFinalResponse> CalcularCalificacionFinalAsync(int idUsuario, int idCurso);

        /// <summary>
        /// Calcula la calificación final de todos los estudiantes de un curso
        /// </summary>
        Task<List<CalificacionFinalResponse>> CalcularCalificacionesCursoAsync(int idCurso);
    }
}