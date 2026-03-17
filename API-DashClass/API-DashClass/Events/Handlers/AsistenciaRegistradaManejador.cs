using API_DashClass.Services.Interfaces;

namespace API_DashClass.Events.Handlers
{
    /// <summary>
    /// Manejador que escucha cuando se registra asistencia
    /// y otorga puntos de gamificación automáticamente
    /// </summary>
    public class AsistenciaRegistradaManejador : IManejadorEvento<AsistenciaRegistradaEvento>
    {
        private readonly IGamificationService _gamificationService;

        public AsistenciaRegistradaManejador(IGamificationService gamificationService)
        {
            _gamificationService = gamificationService;
        }

        public async Task ManejarAsync(AsistenciaRegistradaEvento evento)
        {
            try
            {
                // Registrar puntos por asistencia (5 puntos por defecto)
                await _gamificationService.RegistrarPuntosPorAsistenciaAsync(
                    idSesionAsistencia: evento.IdSesionAsistencia,
                    idUsuario: evento.IdUsuario,
                    idCurso: evento.IdCurso
                );

                Console.WriteLine($"Puntos por asistencia otorgados: Usuario {evento.IdUsuario}, Sesión {evento.IdSesionAsistencia}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al otorgar puntos por asistencia: {ex.Message}");
                // No lanzar excepción para no afectar el registro de asistencia
            }
        }
    }
}