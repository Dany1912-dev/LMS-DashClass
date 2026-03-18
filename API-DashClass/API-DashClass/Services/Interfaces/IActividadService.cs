using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;

namespace API_DashClass.Services.Interfaces
{
    public interface IActividadService
    {
        /// <summary>
        /// Crea una actividad y la asigna a los grupos indicados
        /// (o a todos si IdGrupos es null/vacío)
        /// </summary>
        Task<ActividadResponse> CrearActividadAsync(CrearActividadRequest request);

        /// <summary>
        /// Obtiene todas las actividades de un curso, con filtro opcional por estatus
        /// </summary>
        Task<List<ActividadResponse>> ObtenerActividadesCursoAsync(int idCurso, string? estatus = null);

        /// <summary>
        /// Obtiene actividades de un curso filtradas por grupo, con filtro opcional por estatus
        /// </summary>
        Task<List<ActividadResponse>> ObtenerActividadesGrupoAsync(int idCurso, int idGrupo, string? estatus = null);

        /// <summary>
        /// Obtiene una actividad por su ID
        /// </summary>
        Task<ActividadResponse?> ObtenerActividadPorIdAsync(int idActividad);

        /// <summary>
        /// Actualiza los datos de una actividad existente
        /// </summary>
        Task<ActividadResponse> ActualizarActividadAsync(int idActividad, ActualizarActividadRequest request);

        /// <summary>
        /// Cambia el estatus de una actividad (publicar, archivar, etc.)
        /// </summary>
        Task<bool> CambiarEstatusActividadAsync(int idActividad, string estatus);
    }
}