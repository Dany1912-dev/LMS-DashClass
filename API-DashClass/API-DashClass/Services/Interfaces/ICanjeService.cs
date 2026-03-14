using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;

namespace API_DashClass.Services.Interfaces
{
    public interface ICanjeService
    {
        /// <summary>
        /// Canjea una recompensa (gasta puntos del estudiante)
        /// </summary>
        Task<CanjeResponse> CanjearRecompensaAsync(CanjearRecompensaRequest request);

        /// <summary>
        /// Obtiene los canjes de un estudiante en un curso
        /// </summary>
        Task<List<CanjeResponse>> ObtenerCanjesEstudianteAsync(int idUsuario, int idCurso);

        /// <summary>
        /// Obtiene todos los canjes de un curso (para profesor)
        /// </summary>
        Task<List<CanjeResponse>> ObtenerCanjesCursoAsync(int idCurso, string? estado = null);

        /// <summary>
        /// Obtiene los detalles de un canje específico
        /// </summary>
        Task<CanjeResponse?> ObtenerCanjePorIdAsync(int idCanje);

        /// <summary>
        /// Obtiene un canje por su código
        /// </summary>
        Task<CanjeResponse?> ObtenerCanjePorCodigoAsync(string codigoCanje);

        /// <summary>
        /// Reclama un canje (profesor valida código)
        /// </summary>
        Task<CanjeResponse> ReclamarCanjeAsync(string codigoCanje);

        /// <summary>
        /// Cancela un canje y devuelve los puntos al estudiante
        /// </summary>
        Task<bool> CancelarCanjeAsync(int idCanje, int idUsuarioProfesor);

        /// <summary>
        /// Marca canjes expirados automáticamente
        /// </summary>
        Task MarcarCanjesExpiradosAsync();
    }
}