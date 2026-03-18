using API_DashClass.Data;
using API_DashClass.Models.Entities;
using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;
using API_DashClass.Services.Interfaces;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace API_DashClass.Services.Implementaciones
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UsuarioService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ========================================
        // OBTENER PERFIL
        // ========================================

        public async Task<UsuarioResponse> ObtenerPerfilAsync(int idUsuario)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            return MapearUsuario(usuario);
        }

        // ========================================
        // ACTUALIZAR PERFIL
        // ========================================

        public async Task<UsuarioResponse> ActualizarPerfilAsync(int idUsuario, ActualizarPerfilRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            usuario.Nombre = request.Nombre;
            usuario.Apellidos = request.Apellidos;
            usuario.Biografia = request.Biografia;

            await _context.SaveChangesAsync();
            return MapearUsuario(usuario);
        }

        // ========================================
        // ACTUALIZAR FOTO
        // ========================================

        public async Task<UsuarioResponse> ActualizarFotoAsync(int idUsuario, ActualizarFotoRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            usuario.FotoPerfilUrl = request.FotoPerfilUrl;

            await _context.SaveChangesAsync();
            return MapearUsuario(usuario);
        }

        // ========================================
        // ACTUALIZAR BIOGRAFIA
        // ========================================

        public async Task<UsuarioResponse> ActualizarBiografiaAsync(int idUsuario, string biografia)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            usuario.Biografia = biografia;

            await _context.SaveChangesAsync();
            return MapearUsuario(usuario);
        }

        // ========================================
        // CAMBIAR ESTATUS
        // ========================================

        public async Task<bool> CambiarEstatusAsync(int idUsuario, bool estatus)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null) return false;

            usuario.Estatus = estatus;
            await _context.SaveChangesAsync();
            return true;
        }

        // ========================================
        // OBTENER CURSOS
        // ========================================

        public async Task<List<CursoResponse>> ObtenerCursosAsync(int idUsuario)
        {
            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == idUsuario);
            if (!usuarioExiste)
                throw new InvalidOperationException("Usuario no encontrado");

            var idCursos = await _context.MiembrosCurso
                .Where(m => m.IdUsuario == idUsuario && m.Estatus == true)
                .Select(m => m.IdCurso)
                .Distinct()
                .ToListAsync();

            var responses = new List<CursoResponse>();

            foreach (var idCurso in idCursos)
            {
                var curso = await _context.Cursos
                    .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

                if (curso == null) continue;

                var profesor = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.IdUsuario == curso.IdUsuario);

                var grupos = await _context.Grupos
                    .Where(g => g.IdCurso == idCurso && g.Estatus == true)
                    .ToListAsync();

                var totalEstudiantes = await _context.MiembrosCurso
                    .CountAsync(m => m.IdCurso == idCurso &&
                                     m.Rol == MiembrosCursos.RolMiembro.Estudiante &&
                                     m.Estatus == true);

                responses.Add(new CursoResponse
                {
                    IdCurso = curso.IdCurso,
                    Codigo = curso.Codigo,
                    Nombre = curso.Nombre,
                    Descripcion = curso.Descripcion,
                    ImagenBanner = curso.ImagenBanner,
                    IdUsuario = curso.IdUsuario,
                    NombreProfesor = profesor != null ? $"{profesor.Nombre} {profesor.Apellidos}" : "Desconocido",
                    FechaCreacion = curso.FechaCreacion,
                    Activo = curso.Estatus,
                    TotalEstudiantes = totalEstudiantes,
                    TotalGrupos = grupos.Count,
                    Grupos = new List<GrupoResponse>()
                });
            }

            return responses;
        }

        // ========================================
        // OBTENER LOGROS
        // ========================================

        public async Task<List<LogroUsuarioResponse>> ObtenerLogrosAsync(int idUsuario)
        {
            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == idUsuario);
            if (!usuarioExiste)
                throw new InvalidOperationException("Usuario no encontrado");

            var logrosUsuario = await _context.LogrosUsuario
                .Where(l => l.IdUsuario == idUsuario)
                .ToListAsync();

            var responses = new List<LogroUsuarioResponse>();

            foreach (var logroUsuario in logrosUsuario)
            {
                var logro = await _context.Logros
                    .FirstOrDefaultAsync(l => l.IdLogro == logroUsuario.IdLogro);

                if (logro == null) continue;

                var curso = await _context.Cursos
                    .FirstOrDefaultAsync(c => c.IdCurso == logro.IdCurso);

                var nombreUsuario = await _context.Usuarios
                    .Where(u => u.IdUsuario == idUsuario)
                    .Select(u => u.Nombre + " " + u.Apellidos)
                    .FirstOrDefaultAsync() ?? "Desconocido";

                responses.Add(new LogroUsuarioResponse
                {
                    IdLogroUsuario = logroUsuario.IdLogroUsuario,
                    IdLogro = logro.IdLogro,
                    NombreLogro = logro.Nombre,
                    DescripcionLogro = logro.Descripcion,
                    IconoLogro = logro.UrlIcono,
                    IdUsuario = idUsuario,
                    NombreUsuario = nombreUsuario,
                    FechaDesbloqueo = logroUsuario.FechaDesbloqueo
                });
            }

            return responses;
        }

        // ========================================
        // OBTENER ESTADÍSTICAS
        // ========================================

        public async Task<EstadisticasUsuarioResponse> ObtenerEstadisticasAsync(int idUsuario)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            var totalCursos = await _context.MiembrosCurso
                .CountAsync(m => m.IdUsuario == idUsuario && m.Estatus == true);

            var totalLogros = await _context.LogrosUsuario
                .CountAsync(l => l.IdUsuario == idUsuario);

            var totalCanjes = await _context.Canjes
                .CountAsync(c => c.IdUsuario == idUsuario);

            var totalAsistencias = await _context.RegistrosAsistencia
                .CountAsync(r => r.IdUsuario == idUsuario);

            // Puntos por curso
            var idCursos = await _context.MiembrosCurso
                .Where(m => m.IdUsuario == idUsuario && m.Estatus == true)
                .Select(m => m.IdCurso)
                .Distinct()
                .ToListAsync();

            var puntosPorCurso = new List<PuntosPorCursoResponse>();

            foreach (var idCurso in idCursos)
            {
                var curso = await _context.Cursos
                    .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

                if (curso == null) continue;

                var transacciones = await _context.TransaccionesPuntos
                    .Where(t => t.IdUsuario == idUsuario && t.IdCurso == idCurso)
                    .ToListAsync();

                var puntosGanados = transacciones
                    .Where(t => t.Tipo == TransaccionesPuntos.TipoTransaccion.Ganado)
                    .Sum(t => t.Cantidad);

                var puntosGastados = transacciones
                    .Where(t => t.Tipo == TransaccionesPuntos.TipoTransaccion.Gastado)
                    .Sum(t => t.Cantidad);

                var puntosActuales = transacciones.Any()
                    ? transacciones.OrderByDescending(t => t.FechaCreacion).First().BalanceDespues
                    : 0;

                puntosPorCurso.Add(new PuntosPorCursoResponse
                {
                    IdCurso = idCurso,
                    NombreCurso = curso.Nombre,
                    PuntosActuales = puntosActuales,
                    PuntosGanados = puntosGanados,
                    PuntosGastados = puntosGastados
                });
            }

            return new EstadisticasUsuarioResponse
            {
                IdUsuario = idUsuario,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellidos}",
                TotalCursos = totalCursos,
                TotalLogros = totalLogros,
                TotalCanjes = totalCanjes,
                TotalAsistencias = totalAsistencias,
                PuntosPorCurso = puntosPorCurso
            };
        }

        // ========================================
        // OBTENER MÉTODOS AUTH
        // ========================================

        public async Task<List<MetodoAuthResponse>> ObtenerMetodosAuthAsync(int idUsuario)
        {
            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == idUsuario);
            if (!usuarioExiste)
                throw new InvalidOperationException("Usuario no encontrado");

            var metodos = await _context.MetodosAuthUsers
                .Where(m => m.IdUsuario == idUsuario)
                .ToListAsync();

            return metodos.Select(m => new MetodoAuthResponse
            {
                IdMetodo = m.IdMetodo,
                Proveedor = m.ProveedorAuth.ToString(),
                Email = m.Email,
                Verificado = m.Verificado,
                VinculadoEn = m.VinculadoEn,
                UltimoUso = m.UltimoUso
            }).ToList();
        }

        // ========================================
        // VINCULAR GOOGLE
        // ========================================

        public async Task<MetodoAuthResponse> VincularGoogleAsync(int idUsuario, VincularGoogleRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            var clientId = _configuration["GoogleOAuth:ClientId"]
                ?? throw new InvalidOperationException("Google ClientId no configurado");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            }
            catch
            {
                throw new InvalidOperationException("Token de Google inválido o expirado");
            }

            // Verificar que este Google ID no esté vinculado a otro usuario
            var yaVinculado = await _context.MetodosAuthUsers
                .AnyAsync(m => m.IdUsuarioProveedor == payload.Subject &&
                               m.ProveedorAuth == MetodosAuthUsers.EnumsProveedorAuth.Google &&
                               m.IdUsuario != idUsuario);

            if (yaVinculado)
                throw new InvalidOperationException("Esta cuenta de Google ya está vinculada a otro usuario");

            // Verificar que este usuario no tenga ya Google vinculado
            var metodoExistente = await _context.MetodosAuthUsers
                .FirstOrDefaultAsync(m => m.IdUsuario == idUsuario &&
                                          m.ProveedorAuth == MetodosAuthUsers.EnumsProveedorAuth.Google);

            if (metodoExistente != null)
                throw new InvalidOperationException("Ya tienes Google vinculado a tu cuenta");

            var nuevoMetodo = new MetodosAuthUsers
            {
                IdUsuario = idUsuario,
                ProveedorAuth = MetodosAuthUsers.EnumsProveedorAuth.Google,
                Email = payload.Email,
                IdUsuarioProveedor = payload.Subject,
                Verificado = true,
                VinculadoEn = DateTime.UtcNow,
                UltimoUso = DateTime.UtcNow
            };

            _context.MetodosAuthUsers.Add(nuevoMetodo);
            await _context.SaveChangesAsync();

            return new MetodoAuthResponse
            {
                IdMetodo = nuevoMetodo.IdMetodo,
                Proveedor = nuevoMetodo.ProveedorAuth.ToString(),
                Email = nuevoMetodo.Email,
                Verificado = nuevoMetodo.Verificado,
                VinculadoEn = nuevoMetodo.VinculadoEn,
                UltimoUso = nuevoMetodo.UltimoUso
            };
        }

        // ========================================
        // VINCULAR MICROSOFT
        // ========================================

        public async Task<MetodoAuthResponse> VincularMicrosoftAsync(int idUsuario, VincularMicrosoftRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            var clientId = _configuration["MicrosoftOAuth:ClientId"]
                ?? throw new InvalidOperationException("Microsoft ClientId no configurado");

            string email, microsoftId;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(request.IdToken);

                var aud = jwtToken.Claims.FirstOrDefault(c => c.Type == "aud")?.Value;
                if (aud != clientId)
                    throw new InvalidOperationException("Token no válido para esta aplicación");

                if (jwtToken.ValidTo < DateTime.UtcNow)
                    throw new InvalidOperationException("Token de Microsoft expirado");

                email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == "preferred_username")?.Value
                    ?? throw new InvalidOperationException("No se pudo obtener el email del token");

                microsoftId = jwtToken.Claims.FirstOrDefault(c => c.Type == "oid" || c.Type == "sub")?.Value
                    ?? throw new InvalidOperationException("No se pudo obtener el ID del token");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch
            {
                throw new InvalidOperationException("Token de Microsoft inválido o expirado");
            }

            // Verificar que este Microsoft ID no esté vinculado a otro usuario
            var yaVinculado = await _context.MetodosAuthUsers
                .AnyAsync(m => m.IdUsuarioProveedor == microsoftId &&
                               m.ProveedorAuth == MetodosAuthUsers.EnumsProveedorAuth.Microsoft &&
                               m.IdUsuario != idUsuario);

            if (yaVinculado)
                throw new InvalidOperationException("Esta cuenta de Microsoft ya está vinculada a otro usuario");

            // Verificar que este usuario no tenga ya Microsoft vinculado
            var metodoExistente = await _context.MetodosAuthUsers
                .FirstOrDefaultAsync(m => m.IdUsuario == idUsuario &&
                                          m.ProveedorAuth == MetodosAuthUsers.EnumsProveedorAuth.Microsoft);

            if (metodoExistente != null)
                throw new InvalidOperationException("Ya tienes Microsoft vinculado a tu cuenta");

            var nuevoMetodo = new MetodosAuthUsers
            {
                IdUsuario = idUsuario,
                ProveedorAuth = MetodosAuthUsers.EnumsProveedorAuth.Microsoft,
                Email = email,
                IdUsuarioProveedor = microsoftId,
                Verificado = true,
                VinculadoEn = DateTime.UtcNow,
                UltimoUso = DateTime.UtcNow
            };

            _context.MetodosAuthUsers.Add(nuevoMetodo);
            await _context.SaveChangesAsync();

            return new MetodoAuthResponse
            {
                IdMetodo = nuevoMetodo.IdMetodo,
                Proveedor = nuevoMetodo.ProveedorAuth.ToString(),
                Email = nuevoMetodo.Email,
                Verificado = nuevoMetodo.Verificado,
                VinculadoEn = nuevoMetodo.VinculadoEn,
                UltimoUso = nuevoMetodo.UltimoUso
            };
        }

        // ========================================
        // DESVINCULAR PROVEEDOR
        // ========================================

        public async Task<bool> DesvincularProveedorAsync(int idUsuario, string proveedor)
        {
            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == idUsuario);
            if (!usuarioExiste)
                throw new InvalidOperationException("Usuario no encontrado");

            // Verificar que el usuario tiene más de un método vinculado
            var totalMetodos = await _context.MetodosAuthUsers
                .CountAsync(m => m.IdUsuario == idUsuario);

            if (totalMetodos <= 1)
                throw new InvalidOperationException("No puedes desvincular tu único método de autenticación");

            if (!Enum.TryParse<MetodosAuthUsers.EnumsProveedorAuth>(proveedor, true, out var proveedorEnum))
                throw new InvalidOperationException("Proveedor no válido");

            var metodo = await _context.MetodosAuthUsers
                .FirstOrDefaultAsync(m => m.IdUsuario == idUsuario && m.ProveedorAuth == proveedorEnum);

            if (metodo == null)
                throw new InvalidOperationException("No tienes ese proveedor vinculado");

            _context.MetodosAuthUsers.Remove(metodo);
            await _context.SaveChangesAsync();

            return true;
        }

        // ========================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ========================================

        private static UsuarioResponse MapearUsuario(Usuario usuario)
        {
            return new UsuarioResponse
            {
                IdUsuario = usuario.IdUsuario,
                Email = usuario.Email,
                Nombre = usuario.Nombre,
                Apellidos = usuario.Apellidos,
                FotoPerfilUrl = usuario.FotoPerfilUrl,
                Biografia = usuario.Biografia,
                ProveedorAuthPrincipal = usuario.ProveedorAuthPrincipal,
                FechaCreacion = usuario.FechaCreacion,
                UltimoAcceso = usuario.UltimoAcceso,
                Estatus = usuario.Estatus
            };
        }
    }
}