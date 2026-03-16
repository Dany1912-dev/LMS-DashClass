using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;

namespace API_DashClass.Services.Interfaces
{
    public interface ICursoService
    {
        /// <summary>
        /// Crea un nuevo curso con grupos iniciales y genera invitación
        /// </summary>
        Task<CursoResponse> CrearCursoAsync(CrearCursoRequest request);

        /// <summary>
        /// Obtiene un curso por su ID
        /// </summary>
        Task<CursoResponse?> ObtenerCursoPorIdAsync(int idCurso);

        /// <summary>
        /// Obtiene todos los cursos de un usuario (como profesor o estudiante)
        /// </summary>
        Task<List<CursoResponse>> ObtenerCursosDeUsuarioAsync(int idUsuario);

        /// <summary>
        /// Actualiza los datos de un curso
        /// </summary>
        Task<CursoResponse> ActualizarCursoAsync(int idCurso, ActualizarCursoRequest request);

        /// <summary>
        /// Activa o desactiva un curso
        /// </summary>
        Task<bool> CambiarEstatusCursoAsync(int idCurso, bool estatus);

        /// <summary>
        /// Une a un usuario a un curso usando código o enlace
        /// </summary>
        Task<CursoResponse> UnirseACursoAsync(UnirseACursoRequest request);

        /// <summary>
        /// Obtiene los miembros de un curso
        /// </summary>
        Task<List<MiembroCursoResponse>> ObtenerMiembrosCursoAsync(int idCurso);
    }
}