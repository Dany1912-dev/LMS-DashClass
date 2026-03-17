namespace API_DashClass.Events
{
    /// <summary>
    /// Interfaz para manejadores de eventos
    /// </summary>
    public interface IManejadorEvento<TEvento> where TEvento : IEvento
    {
        Task ManejarAsync(TEvento evento);
    }
}