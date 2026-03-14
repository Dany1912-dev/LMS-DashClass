using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;

namespace API_DashClass.Services.Interfaces
{
    public interface ILogroService
    {
        /// <summary>
        /// Crea un nuevo logro en un curso
        /// </summary>
        Task<LogroResponse> CrearLogroAsync(CrearLogroRequest request);

        /// <summary>
        /// Obtiene todos los logros de un curso
        /// </summary>
        Task<List<LogroResponse>> ObtenerLogrosCursoAsync(int idCurso);

        /// <summary>
        /// Obtiene todos los logros activos de un curso
        /// </summary>
        Task<List<LogroResponse>> ObtenerLogrosActivosAsync(int idCurso);

        /// <summary>
        /// Obtiene los detalles de un logro específico
        /// </summary>
        Task<LogroResponse?> ObtenerLogroPorIdAsync(int idLogro);

        /// <summary>
        /// Actualiza un logro existente
        /// </summary>
        Task<LogroResponse> ActualizarLogroAsync(int idLogro, ActualizarLogroRequest request);

        /// <summary>
        /// Activa o desactiva un logro
        /// </summary>
        Task<bool> CambiarEstatusLogroAsync(int idLogro, bool estatus);

        /// <summary>
        /// Desbloquea un logro para un usuario y otorga puntos
        /// </summary>
        Task<LogroUsuarioResponse> DesbloquearLogroAsync(DesbloquearLogroRequest request);

        /// <summary>
        /// Obtiene todos los logros desbloqueados de un usuario en un curso
        /// </summary>
        Task<List<LogroUsuarioResponse>> ObtenerLogrosUsuarioAsync(int idUsuario, int idCurso);

        /// <summary>
        /// Obtiene el progreso de logros de un usuario (X de Y logros)
        /// </summary>
        Task<ProgresoLogrosResponse> ObtenerProgresoLogrosAsync(int idUsuario, int idCurso);

        /// <summary>
        /// Verifica si un usuario tiene un logro específico
        /// </summary>
        Task<bool> UsuarioTieneLogroAsync(int idUsuario, int idLogro);
    }
}