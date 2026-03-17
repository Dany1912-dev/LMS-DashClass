using API_DashClass.Services.Interfaces;

namespace API_DashClass.Events.Handlers
{
    /// <summary>
    /// Manejador que escucha cuando se crea una calificación
    /// y otorga puntos de gamificación automáticamente
    /// </summary>
    public class CalificacionCreadaManejador : IManejadorEvento<CalificacionCreadaEvento>
    {
        private readonly IGamificationService _gamificationService;

        public CalificacionCreadaManejador(IGamificationService gamificationService)
        {
            _gamificationService = gamificationService;
        }

        public async Task ManejarAsync(CalificacionCreadaEvento evento)
        {
            try
            {
                // Si la actividad no tiene puntos de gamificación, no hacer nada
                if (evento.PuntosGamificacionMaximos <= 0)
                {
                    return;
                }

                // Registrar puntos por calificación
                await _gamificationService.RegistrarPuntosPorCalificacionAsync(
                    idEntrega: evento.IdEntrega,
                    idUsuario: evento.IdUsuario,
                    idCurso: evento.IdCurso,
                    puntuacion: evento.Puntuacion,
                    puntosMaximos: evento.PuntosMaximos,
                    puntosGamificacionMaximos: evento.PuntosGamificacionMaximos
                );

                Console.WriteLine($"Puntos otorgados automáticamente: Usuario {evento.IdUsuario}, Calificación {evento.Puntuacion}/{evento.PuntosMaximos}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al otorgar puntos por calificación: {ex.Message}");
                // No lanzar excepción para no afectar el proceso de calificación
            }
        }
    }
}