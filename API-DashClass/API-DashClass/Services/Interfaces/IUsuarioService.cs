using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;

namespace API_DashClass.Services.Interfaces
{
    public interface IUsuarioService
    {
        /// <summary>
        /// Obtiene el perfil completo de un usuario
        /// </summary>
        Task<UsuarioResponse> ObtenerPerfilAsync(int idUsuario);

        /// <summary>
        /// Actualiza nombre, apellidos y biografía
        /// </summary>
        Task<UsuarioResponse> ActualizarPerfilAsync(int idUsuario, ActualizarPerfilRequest request);

        /// <summary>
        /// Actualiza la foto de perfil
        /// </summary>
        Task<UsuarioResponse> ActualizarFotoAsync(int idUsuario, ActualizarFotoRequest request);

        /// <summary>
        /// Actualiza la biografía
        /// </summary>
        Task<UsuarioResponse> ActualizarBiografiaAsync(int idUsuario, string biografia);

        /// <summary>
        /// Activa o desactiva la cuenta
        /// </summary>
        Task<bool> CambiarEstatusAsync(int idUsuario, bool estatus);

        /// <summary>
        /// Obtiene los cursos del usuario
        /// </summary>
        Task<List<CursoResponse>> ObtenerCursosAsync(int idUsuario);

        /// <summary>
        /// Obtiene los logros desbloqueados del usuario
        /// </summary>
        Task<List<LogroUsuarioResponse>> ObtenerLogrosAsync(int idUsuario);

        /// <summary>
        /// Obtiene las estadísticas del usuario
        /// </summary>
        Task<EstadisticasUsuarioResponse> ObtenerEstadisticasAsync(int idUsuario);

        /// <summary>
        /// Obtiene los métodos de autenticación vinculados
        /// </summary>
        Task<List<MetodoAuthResponse>> ObtenerMetodosAuthAsync(int idUsuario);

        /// <summary>
        /// Vincula Google al usuario
        /// </summary>
        Task<MetodoAuthResponse> VincularGoogleAsync(int idUsuario, VincularGoogleRequest request);

        /// <summary>
        /// Vincula Microsoft al usuario
        /// </summary>
        Task<MetodoAuthResponse> VincularMicrosoftAsync(int idUsuario, VincularMicrosoftRequest request);

        /// <summary>
        /// Desvincula un proveedor del usuario
        /// </summary>
        Task<bool> DesvincularProveedorAsync(int idUsuario, string proveedor);
    }
}