using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;

namespace API_DashClass.Services.Interfaces
{
    public interface ICursoService
    {
        Task<CursoResponse> CrearCursoAsync(CrearCursoRequest request);
        Task<CursoResponse?> ObtenerCursoPorIdAsync(int idCurso);
        Task<List<CursoResponse>> ObtenerCursosDeUsuarioAsync(int idUsuario);
        Task<CursoResponse> ActualizarCursoAsync(int idCurso, ActualizarCursoRequest request);
        Task<bool> CambiarEstatusCursoAsync(int idCurso, bool estatus);
        Task<CursoResponse> UnirseACursoAsync(UnirseACursoRequest request);
        Task<List<MiembroCursoResponse>> ObtenerMiembrosCursoAsync(int idCurso);
        Task<InvitacionCursoResponse> CrearInvitacionAsync(int idCurso, CrearInvitacionRequest request);
        Task<List<InvitacionCursoResponse>> ObtenerInvitacionesAsync(int idCurso);

        /// <summary>
        /// Agrega un nuevo grupo a un curso existente
        /// </summary>
        Task<GrupoResponse> AgregarGrupoAsync(int idCurso, CrearGrupoRequest request);

        /// <summary>
        /// Obtiene todos los grupos de un curso
        /// </summary>
        Task<List<GrupoResponse>> ObtenerGruposPorCursoAsync(int idCurso);
    }
}