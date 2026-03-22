using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;

namespace API_DashClass.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(AuthRegisterRequest request);
        Task<AuthResponse> VerificarEmailAsync(VerificarEmailRequest request);
        Task<AuthResponse> LoginAsync(AuthLoginRequest request);
        Task<AuthResponse> Verificar2FAAsync(Verificar2FARequest request);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string refreshToken);
        Task<AuthResponse> LoginGoogleAsync(GoogleAuthRequest request);
        Task<AuthResponse> LoginMicrosoftAsync(MicrosoftAuthRequest request);
        Task Solicitar2FAAsync(string email);
    }
}