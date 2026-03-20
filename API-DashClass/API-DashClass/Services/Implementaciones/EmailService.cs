using API_DashClass.Services.Interfaces;
using Resend;

namespace API_DashClass.Services.Implementaciones
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _configuration;

        public EmailService(IResend resend, IConfiguration configuration)
        {
            _resend = resend;
            _configuration = configuration;
        }

        public async Task EnviarCodigoVerificacionEmailAsync(string email, string nombre, string codigo)
        {
            var fromEmail = _configuration["ResendSettings:FromEmail"] ?? "onboarding@resend.dev";
            var fromName = _configuration["ResendSettings:FromName"] ?? "Dash Class";

            var mensaje = new EmailMessage
            {
                From = $"{fromName} <{fromEmail}>",
                To = { email },
                Subject = "Verifica tu correo - Dash Class",
                HtmlBody = GenerarHtmlVerificacionEmail(nombre, codigo)
            };

            await _resend.EmailSendAsync(mensaje);
        }

        public async Task EnviarCodigo2FAAsync(string email, string nombre, string codigo)
        {
            var fromEmail = _configuration["ResendSettings:FromEmail"] ?? "onboarding@resend.dev";
            var fromName = _configuration["ResendSettings:FromName"] ?? "Dash Class";

            var mensaje = new EmailMessage
            {
                From = $"{fromName} <{fromEmail}>",
                To = { email },
                Subject = "Código de acceso - Dash Class",
                HtmlBody = GenerarHtml2FA(nombre, codigo)
            };

            await _resend.EmailSendAsync(mensaje);
        }

        private static string GenerarHtmlVerificacionEmail(string nombre, string codigo)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='es'>
            <head>
              <meta charset='UTF-8'>
              <meta name='viewport' content='width=device-width, initial-scale=1.0'>
              <title>Verifica tu correo - Dash Class</title>
            </head>
            <body style='margin:0;padding:0;background-color:#1a1a1a;font-family:Arial,sans-serif;'>
              <div style='max-width:520px;margin:2rem auto;background:#232b1e;border-radius:16px;overflow:hidden;border:1px solid #728156;'>
                <div style='background:#2d3a24;padding:2rem;text-align:center;border-bottom:3px solid #728156;'>
                  <span style='font-size:28px;font-weight:700;color:#88976C;letter-spacing:2px;'>Dash Class</span>
                </div>
                <div style='padding:2rem 2.5rem;'>
                  <p style='color:#CFE1BB;font-size:15px;margin:0 0 0.5rem;'>Hola, <strong style='color:#B6C99C;'>{nombre}</strong> 👋</p>
                  <h2 style='color:#88976C;font-size:20px;font-weight:700;margin:0.5rem 0 1rem;'>Verifica tu correo electrónico</h2>
                  <p style='color:#B6C99C;font-size:14px;line-height:1.7;margin:0 0 1.5rem;'>
                    Usa el siguiente código para completar tu registro en Dash Class.
                    Este código expira en <strong style='color:#CFE1BB;'>15 minutos</strong>.
                  </p>
                  <div style='background:#1a1a1a;border:2px solid #728156;border-radius:12px;padding:1.5rem;text-align:center;margin:1.5rem 0;'>
                    <p style='color:#88976C;font-size:12px;letter-spacing:3px;margin:0 0 0.5rem;text-transform:uppercase;'>Tu código de verificación</p>
                    <span style='font-size:42px;font-weight:700;color:#CFE1BB;letter-spacing:12px;'>{FormatearCodigo(codigo)}</span>
                  </div>
                  <p style='color:#728156;font-size:13px;line-height:1.6;margin:1.5rem 0 0;'>
                    Si no solicitaste este código, ignora este correo. Tu cuenta permanecerá segura.
                  </p>
                </div>
                <div style='background:#2d3a24;padding:1.25rem 2.5rem;border-top:1px solid #728156;text-align:center;'>
                  <p style='color:#728156;font-size:12px;margin:0;'>© 2025 Dash Class · Todos los derechos reservados</p>
                </div>
              </div>
            </body>
            </html>";
        }

        private static string GenerarHtml2FA(string nombre, string codigo)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='es'>
            <head>
              <meta charset='UTF-8'>
              <meta name='viewport' content='width=device-width, initial-scale=1.0'>
              <title>Código de acceso - Dash Class</title>
            </head>
            <body style='margin:0;padding:0;background-color:#1a1a1a;font-family:Arial,sans-serif;'>
              <div style='max-width:520px;margin:2rem auto;background:#232b1e;border-radius:16px;overflow:hidden;border:1px solid #728156;'>
                <div style='background:#2d3a24;padding:2rem;text-align:center;border-bottom:3px solid #728156;'>
                  <span style='font-size:28px;font-weight:700;color:#88976C;letter-spacing:2px;'>Dash Class</span>
                </div>
                <div style='padding:2rem 2.5rem;'>
                  <p style='color:#CFE1BB;font-size:15px;margin:0 0 0.5rem;'>Hola, <strong style='color:#B6C99C;'>{nombre}</strong> 👋</p>
                  <h2 style='color:#88976C;font-size:20px;font-weight:700;margin:0.5rem 0 1rem;'>Código de acceso</h2>
                  <p style='color:#B6C99C;font-size:14px;line-height:1.7;margin:0 0 1.5rem;'>
                    Alguien intentó iniciar sesión en tu cuenta de Dash Class.
                    Si fuiste tú, usa este código. Expira en <strong style='color:#CFE1BB;'>15 minutos</strong>.
                  </p>
                  <div style='background:#1a1a1a;border:2px solid #728156;border-radius:12px;padding:1.5rem;text-align:center;margin:1.5rem 0;'>
                    <p style='color:#88976C;font-size:12px;letter-spacing:3px;margin:0 0 0.5rem;text-transform:uppercase;'>Tu código de acceso</p>
                    <span style='font-size:42px;font-weight:700;color:#CFE1BB;letter-spacing:12px;'>{FormatearCodigo(codigo)}</span>
                  </div>
                  <p style='color:#728156;font-size:13px;line-height:1.6;margin:1.5rem 0 0;'>
                    Si no intentaste iniciar sesión, te recomendamos cambiar tu contraseña inmediatamente.
                  </p>
                </div>
                <div style='background:#2d3a24;padding:1.25rem 2.5rem;border-top:1px solid #728156;text-align:center;'>
                  <p style='color:#728156;font-size:12px;margin:0;'>© 2025 Dash Class · Todos los derechos reservados</p>
                </div>
              </div>
            </body>
            </html>";
        }

        private static string FormatearCodigo(string codigo)
        {
            return string.Join(" ", codigo.ToCharArray());
        }
    }
}