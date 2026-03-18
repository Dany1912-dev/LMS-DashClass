using System.Collections.Concurrent;

namespace API_DashClass.Events
{
    public class BusEventos
    {
        private readonly ConcurrentDictionary<Type, List<Type>> _manejadores = new();
        private readonly IServiceScopeFactory _scopeFactory;

        public BusEventos(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void Suscribir<TEvento, TManejador>()
            where TEvento : IEvento
            where TManejador : IManejadorEvento<TEvento>
        {
            var tipoEvento = typeof(TEvento);
            var tipoManejador = typeof(TManejador);

            if (!_manejadores.ContainsKey(tipoEvento))
                _manejadores[tipoEvento] = new List<Type>();

            if (!_manejadores[tipoEvento].Contains(tipoManejador))
                _manejadores[tipoEvento].Add(tipoManejador);
        }

        public async Task PublicarAsync<TEvento>(TEvento evento) where TEvento : IEvento
        {
            var tipoEvento = typeof(TEvento);

            if (!_manejadores.ContainsKey(tipoEvento)) return;

            foreach (var tipoManejador in _manejadores[tipoEvento])
            {
                // Crear un scope nuevo por cada manejador para resolver servicios Scoped
                using var scope = _scopeFactory.CreateScope();
                var manejador = scope.ServiceProvider.GetService(tipoManejador);

                if (manejador is IManejadorEvento<TEvento> manejadorEvento)
                {
                    try
                    {
                        await manejadorEvento.ManejarAsync(evento);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en manejador {tipoManejador.Name}: {ex.Message}");
                    }
                }
            }
        }
    }
}