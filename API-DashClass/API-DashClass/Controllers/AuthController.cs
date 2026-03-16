using API_DashClass.Models.Request;
using API_DashClass.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_DashClass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthRegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var response = await _authService.RegisterAsync(request);
                return Ok(new { message = response.Mensaje ?? "Usuario registrado exitosamente", data = response });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al registrar usuario", error = ex.Message }); }
        }

        /// <summary>
        /// Verifica el email con el código enviado al registrarse
        /// </summary>
        [HttpPost("verificar-email")]
        public async Task<IActionResult> VerificarEmail([FromBody] VerificarEmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var response = await _authService.VerificarEmailAsync(request);
                return Ok(new { message = "Email verificado exitosamente", data = response });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al verificar email", error = ex.Message }); }
        }

        /// <summary>
        /// Inicia sesión con email y contraseña
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthLoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var response = await _authService.LoginAsync(request);
                return Ok(new { message = response.Mensaje ?? "Inicio de sesión exitoso", data = response });
            }
            catch (InvalidOperationException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al iniciar sesión", error = ex.Message }); }
        }

        /// <summary>
        /// Verifica el código 2FA y devuelve JWT
        /// </summary>
        [HttpPost("verificar-2fa")]
        public async Task<IActionResult> Verificar2FA([FromBody] Verificar2FARequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var response = await _authService.Verificar2FAAsync(request);
                return Ok(new { message = "Inicio de sesión exitoso", data = response });
            }
            catch (InvalidOperationException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al verificar código", error = ex.Message }); }
        }

        /// <summary>
        /// Inicia sesión o registra un usuario con Google
        /// </summary>
        [HttpPost("google")]
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleAuthRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var response = await _authService.LoginGoogleAsync(request);
                return Ok(new { message = "Autenticación con Google exitosa", data = response });
            }
            catch (InvalidOperationException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al autenticar con Google", error = ex.Message }); }
        }

        /// <summary>
        /// Inicia sesión o registra un usuario con Microsoft
        /// </summary>
        [HttpPost("microsoft")]
        public async Task<IActionResult> LoginMicrosoft([FromBody] MicrosoftAuthRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var response = await _authService.LoginMicrosoftAsync(request);
                return Ok(new { message = "Autenticación con Microsoft exitosa", data = response });
            }
            catch (InvalidOperationException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al autenticar con Microsoft", error = ex.Message }); }
        }

        /// <summary>
        /// Renueva el AccessToken usando un RefreshToken válido
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken)) return BadRequest(new { message = "El refresh token es requerido" });
                var response = await _authService.RefreshTokenAsync(refreshToken);
                return Ok(new { message = "Token renovado exitosamente", data = response });
            }
            catch (InvalidOperationException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al renovar token", error = ex.Message }); }
        }
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken)) return BadRequest(new { message = "El refresh token es requerido" });
                var resultado = await _authService.LogoutAsync(refreshToken);
                if (!resultado) return NotFound(new { message = "Refresh token no encontrado" });
                return Ok(new { message = "Sesión cerrada exitosamente" });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al cerrar sesión", error = ex.Message }); }
        }
    }
}