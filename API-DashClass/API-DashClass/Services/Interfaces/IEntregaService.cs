using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;

namespace API_DashClass.Services.Interfaces
{
    public interface IEntregaService
    {
        /// <summary>Marcar actividad como entregada sin archivos</summary>
        Task<EntregaResponse> MarcarEntregadaAsync(MarcarEntregadaRequest request);

        /// <summary>Entregar con archivos adjuntos</summary>
        Task<EntregaResponse> SubirEntregaAsync(SubirEntregaRequest request);

        /// <summary>Obtiene la entrega activa de un estudiante en una actividad</summary>
        Task<EntregaResponse?> ObtenerEntregaEstudianteAsync(int idActividad, int idUsuario);

        /// <summary>Obtiene todas las entregas de una actividad (para el profesor)</summary>
        Task<List<EntregaResponse>> ObtenerEntregasActividadAsync(int idActividad);

        /// <summary>Califica una entrega</summary>
        Task<CalificacionResponse> CalificarEntregaAsync(int idEntrega, CalificarEntregaRequest request);

        /// <summary>Obtiene la calificación de una entrega</summary>
        Task<CalificacionResponse?> ObtenerCalificacionAsync(int idEntrega);
    }
}