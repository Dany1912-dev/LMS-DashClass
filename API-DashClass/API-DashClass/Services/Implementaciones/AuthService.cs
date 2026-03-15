using API_DashClass.Data;
using API_DashClass.Models.Entities;
using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;
using API_DashClass.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API_DashClass.Services.Implementaciones
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public AuthService(AppDbContext context, IConfiguration configuration, IEmailService emailService, IMemoryCache cache)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _cache = cache;
        }

        // ========================================
        // REGISTER
        // ========================================

        public async Task<AuthResponse> RegisterAsync(AuthRegisterRequest request)
        {
            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == request.Email);

            if (emailExiste)
                throw new InvalidOperationException("El email ya está registrado");

            var enableEmailVerification = _configuration.GetValue<bool>("AuthSettings:EnableEmailVerification");

            var nuevoUsuario = new Usuario
            {
                Email = request.Email,
                Nombre = request.Nombre,
                Apellidos = request.Apellidos,
                ProveedorAuthPrincipal = "Local",
                FechaCreacion = DateTime.UtcNow,
                UltimoAcceso = DateTime.UtcNow,
                Estatus = true
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            var metodoAuth = new MetodosAuthUsers
            {
                IdUsuario = nuevoUsuario.IdUsuario,
                ProveedorAuth = MetodosAuthUsers.EnumsProveedorAuth.Local,
                Email = request.Email,
                ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(request.Contrasena),
                Verificado = !enableEmailVerification,
                VinculadoEn = DateTime.UtcNow
            };

            _context.MetodosAuthUsers.Add(metodoAuth);
            await _context.SaveChangesAsync();

            if (enableEmailVerification)
            {
                var codigo = GenerarCodigo6Digitos();
                _cache.Set($"verify_email_{request.Email}", codigo, TimeSpan.FromMinutes(15));
                await _emailService.EnviarCodigoVerificacionEmailAsync(request.Email, request.Nombre, codigo);
                return new AuthResponse { Mensaje = "Registro exitoso. Revisa tu correo para verificar tu cuenta." };
            }

            return await GenerarAuthResponseAsync(nuevoUsuario);
        }

        // ========================================
        // VERIFICAR EMAIL
        // ========================================

        public async Task<AuthResponse> VerificarEmailAsync(VerificarEmailRequest request)
        {
            var cacheKey = $"verify_email_{request.Email}";

            if (!_cache.TryGetValue(cacheKey, out string? codigoGuardado))
                throw new InvalidOperationException("El código ha expirado. Solicita uno nuevo.");

            if (codigoGuardado != request.Codigo)
                throw new InvalidOperationException("Código incorrecto");

            var metodoAuth = await _context.MetodosAuthUsers
                .FirstOrDefaultAsync(m => m.Email == request.Email &&
                                          m.ProveedorAuth == MetodosAuthUsers.EnumsProveedorAuth.Local)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            metodoAuth.Verificado = true;
            metodoAuth.UltimoUso = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _cache.Remove(cacheKey);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            usuario.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GenerarAuthResponseAsync(usuario);
        }

        // ========================================
        // LOGIN
        // ========================================

        public async Task<AuthResponse> LoginAsync(AuthLoginRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email)
                ?? throw new InvalidOperationException("Credenciales inválidas");

            if (!usuario.Estatus)
                throw new InvalidOperationException("La cuenta está desactivada");

            var metodoAuth = await _context.MetodosAuthUsers
                .FirstOrDefaultAsync(m => m.IdUsuario == usuario.IdUsuario &&
                                          m.ProveedorAuth == MetodosAuthUsers.EnumsProveedorAuth.Local);

            if (metodoAuth == null || metodoAuth.ContrasenaHash == null)
                throw new InvalidOperationException("Credenciales inválidas");

            if (!metodoAuth.Verificado)
                throw new InvalidOperationException("Debes verificar tu correo antes de iniciar sesión");

            if (!BCrypt.Net.BCrypt.Verify(request.Contrasena, metodoAuth.ContrasenaHash))
                throw new InvalidOperationException("Credenciales inválidas");

            usuario.UltimoAcceso = DateTime.UtcNow;
            metodoAuth.UltimoUso = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var enable2FA = _configuration.GetValue<bool>("AuthSettings:Enable2FA");

            if (enable2FA)
            {
                var codigo = GenerarCodigo6Digitos();
                _cache.Set($"2fa_{request.Email}", codigo, TimeSpan.FromMinutes(15));
                await _emailService.EnviarCodigo2FAAsync(request.Email, usuario.Nombre, codigo);
                return new AuthResponse { Mensaje = "Credenciales válidas. Revisa tu correo para obtener el código de acceso." };
            }

            return await GenerarAuthResponseAsync(usuario);
        }

        // ========================================
        // VERIFICAR 2FA
        // ========================================

        public async Task<AuthResponse> Verificar2FAAsync(Verificar2FARequest request)
        {
            var cacheKey = $"2fa_{request.Email}";

            if (!_cache.TryGetValue(cacheKey, out string? codigoGuardado))
                throw new InvalidOperationException("El código ha expirado. Inicia sesión de nuevo.");

            if (codigoGuardado != request.Codigo)
                throw new InvalidOperationException("Código incorrecto");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            usuario.UltimoAcceso = DateTime.UtcNow;

            var metodoAuth = await _context.MetodosAuthUsers
                .FirstOrDefaultAsync(m => m.IdUsuario == usuario.IdUsuario &&
                                          m.ProveedorAuth == MetodosAuthUsers.EnumsProveedorAuth.Local);

            if (metodoAuth != null)
                metodoAuth.UltimoUso = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _cache.Remove(cacheKey);

            return await GenerarAuthResponseAsync(usuario);
        }

        // ========================================
        // REFRESH TOKEN
        // ========================================

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await _context.RefreshTokens
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (tokenEntity == null)
                throw new InvalidOperationException("Refresh token inválido");

            if (tokenEntity.Revocado)
                throw new InvalidOperationException("Refresh token revocado");

            if (tokenEntity.Expiracion < DateTime.UtcNow)
                throw new InvalidOperationException("Refresh token expirado");

            if (tokenEntity.Usuario == null || !tokenEntity.Usuario.Estatus)
                throw new InvalidOperationException("Usuario no encontrado o desactivado");

            tokenEntity.Revocado = true;
            await _context.SaveChangesAsync();

            return await GenerarAuthResponseAsync(tokenEntity.Usuario);
        }

        // ========================================
        // LOGOUT
        // ========================================

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var tokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (tokenEntity == null)
                return false;

            tokenEntity.Revocado = true;
            await _context.SaveChangesAsync();

            return true;
        }

        // ========================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ========================================

        private async Task<AuthResponse> GenerarAuthResponseAsync(Usuario usuario)
        {
            var expirationMinutes = int.Parse(
                _configuration["JwtSettings:ExpirationMinutes"] ?? "15");

            var accessToken = GenerarJwt(usuario);
            var refreshToken = await GenerarRefreshTokenAsync(usuario.IdUsuario);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiracion = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Usuario = new UsuarioAuthResponse
                {
                    IdUsuario = usuario.IdUsuario,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre,
                    Apellidos = usuario.Apellidos,
                    FotoPerfilUrl = usuario.FotoPerfilUrl
                }
            };
        }

        private string GenerarJwt(Usuario usuario)
        {
            var jwtKey = _configuration["JwtSettings:Secret"]
                ?? throw new InvalidOperationException("JWT Secret no configurado");
            var jwtIssuer = _configuration["JwtSettings:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer no configurado");
            var jwtAudience = _configuration["JwtSettings:Audience"]
                ?? throw new InvalidOperationException("JWT Audience no configurado");
            var expirationMinutes = int.Parse(
                _configuration["JwtSettings:ExpirationMinutes"] ?? "15");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim("nombre", usuario.Nombre),
                new Claim("apellidos", usuario.Apellidos),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GenerarRefreshTokenAsync(int idUsuario)
        {
            var refreshDays = int.Parse(
                _configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");

            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(tokenBytes);

            _context.RefreshTokens.Add(new RefreshToken
            {
                IdUsuario = idUsuario,
                Token = token,
                Expiracion = DateTime.UtcNow.AddDays(refreshDays),
                CreadoEn = DateTime.UtcNow,
                Revocado = false
            });

            await _context.SaveChangesAsync();
            return token;
        }

        private static string GenerarCodigo6Digitos()
        {
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }
    }
}