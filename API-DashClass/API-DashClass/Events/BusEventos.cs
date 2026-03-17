using System.Collections.Concurrent;

namespace API_DashClass.Events
{
    /// <summary>
    /// Bus de eventos simple para publicar y suscribirse a eventos
    /// </summary>
    public class BusEventos
    {
        private readonly ConcurrentDictionary<Type, List<Type>> _manejadores = new();
        private readonly IServiceProvider _serviceProvider;

        public BusEventos(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Registra un manejador para un tipo de evento
        /// </summary>
        public void Suscribir<TEvento, TManejador>()
            where TEvento : IEvento
            where TManejador : IManejadorEvento<TEvento>
        {
            var tipoEvento = typeof(TEvento);
            var tipoManejador = typeof(TManejador);

            if (!_manejadores.ContainsKey(tipoEvento))
            {
                _manejadores[tipoEvento] = new List<Type>();
            }

            if (!_manejadores[tipoEvento].Contains(tipoManejador))
            {
                _manejadores[tipoEvento].Add(tipoManejador);
            }
        }

        /// <summary>
        /// Publica un evento y ejecuta todos los manejadores registrados
        /// </summary>
        public async Task PublicarAsync<TEvento>(TEvento evento) where TEvento : IEvento
        {
            var tipoEvento = typeof(TEvento);

            if (!_manejadores.ContainsKey(tipoEvento))
            {
                return; // No hay manejadores registrados para este evento
            }

            var tiposManejadores = _manejadores[tipoEvento];

            foreach (var tipoManejador in tiposManejadores)
            {
                var manejador = _serviceProvider.GetService(tipoManejador);

                if (manejador is IManejadorEvento<TEvento> manejadorEvento)
                {
                    try
                    {
                        await manejadorEvento.ManejarAsync(evento);
                    }
                    catch (Exception ex)
                    {
                        // Log error pero no detener ejecución de otros manejadores
                        Console.WriteLine($"Error en manejador {tipoManejador.Name}: {ex.Message}");
                    }
                }
            }
        }
    }
}