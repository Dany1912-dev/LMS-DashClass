namespace API_DashClass.Services.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Manda el código de verificación de email al registrarse
        /// </summary>
        Task EnviarCodigoVerificacionEmailAsync(string email, string nombre, string codigo);

        /// <summary>
        /// Manda el código de verificación 2FA al iniciar sesión
        /// </summary>
        Task EnviarCodigo2FAAsync(string email, string nombre, string codigo);
    }
}