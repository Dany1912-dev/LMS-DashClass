using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;

namespace API_DashClass.Services.Interfaces
{
    public interface IRecompensaService
    {
        /// <summary>
        /// Crea una nueva recompensa en un curso
        /// </summary>
        Task<RecompensaResponse> CrearRecompensaAsync(CrearRecompensaRequest request);

        /// <summary>
        /// Obtiene todas las recompensas activas de un curso
        /// </summary>
        Task<List<RecompensaResponse>> ObtenerRecompensasActivasAsync(int idCurso);

        /// <summary>
        /// Obtiene todas las recompensas de un curso (incluyendo inactivas)
        /// </summary>
        Task<List<RecompensaResponse>> ObtenerTodasRecompensasAsync(int idCurso);

        /// <summary>
        /// Obtiene los detalles de una recompensa específica
        /// </summary>
        Task<RecompensaResponse?> ObtenerRecompensaPorIdAsync(int idRecompensa);

        /// <summary>
        /// Actualiza una recompensa existente
        /// </summary>
        Task<RecompensaResponse> ActualizarRecompensaAsync(int idRecompensa, ActualizarRecompensaRequest request);

        /// <summary>
        /// Activa o desactiva una recompensa
        /// </summary>
        Task<bool> CambiarEstatusRecompensaAsync(int idRecompensa, bool estatus);

        /// <summary>
        /// Marca una recompensa como destacada o no
        /// </summary>
        Task<bool> CambiarDestacadoRecompensaAsync(int idRecompensa, bool destacado);

        /// <summary>
        /// Incrementa el stock de una recompensa
        /// </summary>
        Task<bool> AgregarStockAsync(int idRecompensa, int cantidad);

        /// <summary>
        /// Reduce el stock de una recompensa (usado al canjear)
        /// </summary>
        Task<bool> ReducirStockAsync(int idRecompensa, int cantidad);
    }
}