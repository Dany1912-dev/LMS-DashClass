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

        /// <summary>
        /// Si EnableEmailVerification = true:
        ///   Guarda datos en caché, manda código, NO crea el usuario todavía.
        /// Si EnableEmailVerification = false:
        ///   Crea el usuario directo y devuelve JWT.
        /// </summary>
        public async Task<AuthResponse> RegisterAsync(AuthRegisterRequest request)
        {
            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == request.Email);

            if (emailExiste)
                throw new InvalidOperationException("El email ya está registrado");

            var enableEmailVerification = _configuration.GetValue<bool>("AuthSettings:EnableEmailVerification");

            if (enableEmailVerification)
            {
                // Verificar si ya hay un registro pendiente para este email
                var cacheKey = $"verify_email_{request.Email}";
                if (_cache.TryGetValue(cacheKey, out _))
                    throw new InvalidOperationException("Ya se mandó un código a este correo. Espera 15 minutos o verifica tu bandeja.");

                // Guardar datos del usuario en caché temporalmente
                var datosRegistro = new DatosRegistroPendiente
                {
                    Email = request.Email,
                    Nombre = request.Nombre,
                    Apellidos = request.Apellidos,
                    ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(request.Contrasena)
                };

                var codigo = GenerarCodigo6Digitos();
                _cache.Set(cacheKey, new DatosVerificacionEmail
                {
                    Codigo = codigo,
                    DatosRegistro = datosRegistro
                }, TimeSpan.FromMinutes(15));

                await _emailService.EnviarCodigoVerificacionEmailAsync(request.Email, request.Nombre, codigo);

                return new AuthResponse { Mensaje = "Código enviado. Revisa tu correo para completar el registro." };
            }

            // Modo simple: crear usuario directo
            var nuevoUsuario = await CrearUsuarioAsync(request.Email, request.Nombre, request.Apellidos,
                BCrypt.Net.BCrypt.HashPassword(request.Contrasena), verificado: true);

            return await GenerarAuthResponseAsync(nuevoUsuario);
        }

        // ========================================
        // VERIFICAR EMAIL
        // ========================================

        /// <summary>
        /// Verifica el código, crea el usuario en BD y devuelve JWT
        /// </summary>
        public async Task<AuthResponse> VerificarEmailAsync(VerificarEmailRequest request)
        {
            var cacheKey = $"verify_email_{request.Email}";

            if (!_cache.TryGetValue(cacheKey, out DatosVerificacionEmail? datosCache) || datosCache == null)
                throw new InvalidOperationException("El código ha expirado. Regístrate de nuevo.");

            if (datosCache.Codigo != request.Codigo)
                throw new InvalidOperationException("Código incorrecto");

            // Verificar que el email no se haya registrado mientras esperaba
            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == request.Email);

            if (emailExiste)
                throw new InvalidOperationException("El email ya está registrado");

            // Ahora sí crear el usuario en BD
            var nuevoUsuario = await CrearUsuarioAsync(
                datosCache.DatosRegistro.Email,
                datosCache.DatosRegistro.Nombre,
                datosCache.DatosRegistro.Apellidos,
                datosCache.DatosRegistro.ContrasenaHash,
                verificado: true
            );

            _cache.Remove(cacheKey);

            return await GenerarAuthResponseAsync(nuevoUsuario);
        }

        // ========================================
        // LOGIN
        // ========================================

        /// <summary>
        /// Si Enable2FA = true: manda código 2FA, no devuelve JWT todavía.
        /// Si Enable2FA = false: devuelve JWT directo.
        /// </summary>
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

        /// <summary>
        /// Crea el usuario y su método de auth Local en BD
        /// </summary>
        private async Task<Usuario> CrearUsuarioAsync(string email, string nombre, string apellidos, string contrasenaHash, bool verificado)
        {
            var nuevoUsuario = new Usuario
            {
                Email = email,
                Nombre = nombre,
                Apellidos = apellidos,
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
                Email = email,
                ContrasenaHash = contrasenaHash,
                Verificado = verificado,
                VinculadoEn = DateTime.UtcNow
            };

            _context.MetodosAuthUsers.Add(metodoAuth);
            await _context.SaveChangesAsync();

            return nuevoUsuario;
        }

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

    // ========================================
    // CLASES AUXILIARES PARA CACHÉ
    // ========================================

    public class DatosRegistroPendiente
    {
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string ContrasenaHash { get; set; } = string.Empty;
    }

    public class DatosVerificacionEmail
    {
        public string Codigo { get; set; } = string.Empty;
        public DatosRegistroPendiente DatosRegistro { get; set; } = new();
    }
}