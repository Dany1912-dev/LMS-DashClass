using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;

namespace API_DashClass.Services.Interfaces
{
    public interface IGamificationService
    {
        /// <summary>
        /// Obtiene el balance actual de puntos de un usuario en un curso
        /// </summary>
        Task<BalanceResponse?> ObtenerBalanceAsync(int idUsuario, int idCurso);

        /// <summary>
        /// Obtiene el historial de transacciones de puntos de un usuario en un curso
        /// </summary>
        Task<List<TransaccionResponse>> ObtenerHistorialAsync(int idUsuario, int idCurso, int limite = 50);

        /// <summary>
        /// Registra puntos manualmente (por el profesor)
        /// </summary>
        Task<TransaccionResponse> RegistrarPuntosManualAsync(ManualPointsRequest request);

        /// <summary>
        /// Transfiere puntos de un usuario a otro dentro del mismo curso
        /// </summary>
        Task<TransferResponse> TransferirPuntosAsync(TransferPointsRequest request);

        /// <summary>
        /// Obtiene el top ranking de estudiantes por puntos en un curso
        /// </summary>
        Task<List<RankingResponse>> ObtenerRankingAsync(int idCurso, int top = 10);

        /// <summary>
        /// Calcula y registra puntos por calificación (llamado por evento)
        /// </summary>
        Task RegistrarPuntosPorCalificacionAsync(int idEntrega, int idUsuario, int idCurso, decimal puntuacion, int puntosMaximos, int puntosGamificacionMaximos);

        /// <summary>
        /// Registra puntos por asistencia (llamado por evento)
        /// </summary>
        Task RegistrarPuntosPorAsistenciaAsync(int idSesionAsistencia, int idUsuario, int idCurso);
    }
}
