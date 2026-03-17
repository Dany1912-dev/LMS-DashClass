namespace API_DashClass.Events
{
    /// <summary>
    /// Interfaz base para todos los eventos del sistema
    /// </summary>
    public interface IEvento
    {
        DateTime OcurrioEn { get; }
    }
}