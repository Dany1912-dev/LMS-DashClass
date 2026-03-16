using API_DashClass.Data;
using API_DashClass.Models.Entities;
using API_DashClass.Models.Request;
using API_DashClass.Models.Responses;
using API_DashClass.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace API_DashClass.Services.Implementaciones
{
    public class CursoService : ICursoService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public CursoService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ========================================
        // CREAR CURSO
        // ========================================

        /// <summary>
        /// Crea un nuevo curso, sus grupos iniciales, la invitación y registra al creador como profesor
        /// </summary>
        public async Task<CursoResponse> CrearCursoAsync(CrearCursoRequest request)
        {
            // Verificar que el usuario existe
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == request.IdUsuario)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            // Generar código único para el curso
            var codigo = await GenerarCodigoUnicoAsync();

            // Crear el curso
            var nuevoCurso = new Cursos
            {
                Codigo = codigo,
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                ImagenBanner = request.ImagenBanner,
                IdUsuario = request.IdUsuario,
                FechaCreacion = DateTime.UtcNow,
                Estatus = true
            };

            _context.Cursos.Add(nuevoCurso);
            await _context.SaveChangesAsync();

            // Crear grupos iniciales
            var gruposCreados = new List<Grupos>();
            foreach (var grupoRequest in request.Grupos)
            {
                var grupo = new Grupos
                {
                    IdCurso = nuevoCurso.IdCurso,
                    Nombre = grupoRequest.Nombre,
                    Descripcion = grupoRequest.Descripcion,
                    FechaCreacion = DateTime.UtcNow,
                    Estatus = true
                };
                _context.Grupos.Add(grupo);
                gruposCreados.Add(grupo);
            }

            await _context.SaveChangesAsync();

            // Registrar al creador como profesor del curso
            _context.MiembrosCurso.Add(new MiembrosCursos
            {
                IdCurso = nuevoCurso.IdCurso,
                IdUsuario = request.IdUsuario,
                IdGrupo = null,
                Rol = MiembrosCursos.RolMiembro.Profesor,
                FechaInscripcion = DateTime.UtcNow,
                Estatus = true
            });

            await _context.SaveChangesAsync();

            // Generar invitación con código de 6 dígitos y enlace
            var codigoInvitacion = GenerarCodigoInvitacion();
            var tokenEnlace = GenerarTokenEnlace();

            // Invitación por código
            _context.InvitacionesCurso.Add(new InvitacionesCurso
            {
                IdCurso = nuevoCurso.IdCurso,
                Tipo = InvitacionesCurso.TipoInvitacion.Codigo,
                Codigo = codigoInvitacion,
                FechaCreacion = DateTime.UtcNow,
                Estatus = true
            });

            // Invitación por enlace
            _context.InvitacionesCurso.Add(new InvitacionesCurso
            {
                IdCurso = nuevoCurso.IdCurso,
                Tipo = InvitacionesCurso.TipoInvitacion.Enlace,
                Token = tokenEnlace,
                FechaCreacion = DateTime.UtcNow,
                Estatus = true
            });

            await _context.SaveChangesAsync();

            return await MapearCursoAResponseAsync(nuevoCurso, usuario, gruposCreados, codigoInvitacion, tokenEnlace);
        }

        // ========================================
        // OBTENER CURSO POR ID
        // ========================================

        public async Task<CursoResponse?> ObtenerCursoPorIdAsync(int idCurso)
        {
            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

            if (curso == null) return null;

            var profesor = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == curso.IdUsuario);

            var grupos = await _context.Grupos
                .Where(g => g.IdCurso == idCurso && g.Estatus == true)
                .ToListAsync();

            var invitacionCodigo = await _context.InvitacionesCurso
                .FirstOrDefaultAsync(i => i.IdCurso == idCurso &&
                                          i.Tipo == InvitacionesCurso.TipoInvitacion.Codigo &&
                                          i.Estatus == true);

            var invitacionEnlace = await _context.InvitacionesCurso
                .FirstOrDefaultAsync(i => i.IdCurso == idCurso &&
                                          i.Tipo == InvitacionesCurso.TipoInvitacion.Enlace &&
                                          i.Estatus == true);

            return await MapearCursoAResponseAsync(curso, profesor, grupos,
                invitacionCodigo?.Codigo, invitacionEnlace?.Token);
        }

        // ========================================
        // OBTENER CURSOS DE USUARIO
        // ========================================

        public async Task<List<CursoResponse>> ObtenerCursosDeUsuarioAsync(int idUsuario)
        {
            var idCursos = await _context.MiembrosCurso
                .Where(m => m.IdUsuario == idUsuario && m.Estatus == true)
                .Select(m => m.IdCurso)
                .Distinct()
                .ToListAsync();

            var responses = new List<CursoResponse>();

            foreach (var idCurso in idCursos)
            {
                var response = await ObtenerCursoPorIdAsync(idCurso);
                if (response != null)
                    responses.Add(response);
            }

            return responses;
        }

        // ========================================
        // ACTUALIZAR CURSO
        // ========================================

        public async Task<CursoResponse> ActualizarCursoAsync(int idCurso, ActualizarCursoRequest request)
        {
            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == idCurso)
                ?? throw new InvalidOperationException("Curso no encontrado");

            curso.Nombre = request.Nombre;
            curso.Descripcion = request.Descripcion;
            curso.ImagenBanner = request.ImagenBanner;

            await _context.SaveChangesAsync();

            return await ObtenerCursoPorIdAsync(idCurso)
                ?? throw new InvalidOperationException("Error al obtener el curso actualizado");
        }

        // ========================================
        // CAMBIAR ESTATUS
        // ========================================

        public async Task<bool> CambiarEstatusCursoAsync(int idCurso, bool estatus)
        {
            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

            if (curso == null) return false;

            curso.Estatus = estatus;
            await _context.SaveChangesAsync();

            return true;
        }

        // ========================================
        // UNIRSE A CURSO
        // ========================================

        public async Task<CursoResponse> UnirseACursoAsync(UnirseACursoRequest request)
        {
            // Verificar que el usuario existe
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == request.IdUsuario)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            // Buscar la invitación por código o token
            var invitacion = await _context.InvitacionesCurso
                .FirstOrDefaultAsync(i =>
                    (i.Codigo == request.CodigoOToken || i.Token == request.CodigoOToken) &&
                    i.Estatus == true);

            if (invitacion == null)
                throw new InvalidOperationException("Código o enlace inválido o expirado");

            // Verificar que el curso está activo
            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == invitacion.IdCurso && c.Estatus == true)
                ?? throw new InvalidOperationException("El curso no está disponible");

            // Verificar que el usuario no sea ya miembro
            var yaEsMiembro = await _context.MiembrosCurso
                .AnyAsync(m => m.IdCurso == curso.IdCurso &&
                               m.IdUsuario == request.IdUsuario &&
                               m.Estatus == true);

            if (yaEsMiembro)
                throw new InvalidOperationException("Ya eres miembro de este curso");

            // Verificar que el grupo existe si se mandó
            if (request.IdGrupo.HasValue)
            {
                var grupoExiste = await _context.Grupos
                    .AnyAsync(g => g.IdGrupo == request.IdGrupo && g.IdCurso == curso.IdCurso);

                if (!grupoExiste)
                    throw new InvalidOperationException("El grupo especificado no existe en este curso");
            }

            // Registrar al usuario como estudiante
            _context.MiembrosCurso.Add(new MiembrosCursos
            {
                IdCurso = curso.IdCurso,
                IdUsuario = request.IdUsuario,
                IdGrupo = request.IdGrupo,
                Rol = MiembrosCursos.RolMiembro.Estudiante,
                FechaInscripcion = DateTime.UtcNow,
                Estatus = true
            });

            await _context.SaveChangesAsync();

            return await ObtenerCursoPorIdAsync(curso.IdCurso)
                ?? throw new InvalidOperationException("Error al obtener el curso");
        }

        // ========================================
        // OBTENER MIEMBROS
        // ========================================

        public async Task<List<MiembroCursoResponse>> ObtenerMiembrosCursoAsync(int idCurso)
        {
            var miembros = await _context.MiembrosCurso
                .Where(m => m.IdCurso == idCurso && m.Estatus == true)
                .ToListAsync();

            var responses = new List<MiembroCursoResponse>();

            foreach (var miembro in miembros)
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.IdUsuario == miembro.IdUsuario);

                if (usuario == null) continue;

                var grupo = miembro.IdGrupo.HasValue
                    ? await _context.Grupos.FirstOrDefaultAsync(g => g.IdGrupo == miembro.IdGrupo)
                    : null;

                responses.Add(new MiembroCursoResponse
                {
                    IdMiembroCurso = miembro.IdMiembroCurso,
                    IdUsuario = miembro.IdUsuario,
                    NombreCompleto = $"{usuario.Nombre} {usuario.Apellidos}",
                    Email = usuario.Email,
                    FotoPerfilUrl = usuario.FotoPerfilUrl,
                    Rol = miembro.Rol.ToString(),
                    NombreGrupo = grupo?.Nombre,
                    FechaInscripcion = miembro.FechaInscripcion,
                    Estatus = miembro.Estatus
                });
            }

            return responses;
        }

        // ========================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ========================================

        /// <summary>
        /// Genera un código único de 8 caracteres alfanuméricos para el curso
        /// </summary>
        private async Task<string> GenerarCodigoUnicoAsync()
        {
            string codigo;
            bool existe;

            do
            {
                codigo = GenerarStringAleatorio(8).ToUpper();
                existe = await _context.Cursos.AnyAsync(c => c.Codigo == codigo);
            } while (existe);

            return codigo;
        }

        /// <summary>
        /// Genera un código de 6 dígitos para invitación
        /// </summary>
        private static string GenerarCodigoInvitacion()
        {
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }

        /// <summary>
        /// Genera un token seguro para el enlace de invitación
        /// </summary>
        private static string GenerarTokenEnlace()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        /// <summary>
        /// Genera un string aleatorio alfanumérico
        /// </summary>
        private static string GenerarStringAleatorio(int longitud)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var resultado = new char[longitud];
            for (int i = 0; i < longitud; i++)
                resultado[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            return new string(resultado);
        }

        /// <summary>
        /// Mapea una entidad Curso a CursoResponse
        /// </summary>
        private async Task<CursoResponse> MapearCursoAResponseAsync(
            Cursos curso, Usuario? profesor, List<Grupos> grupos,
            string? codigoInvitacion, string? tokenEnlace)
        {
            var totalEstudiantes = await _context.MiembrosCurso
                .CountAsync(m => m.IdCurso == curso.IdCurso &&
                                 m.Rol == MiembrosCursos.RolMiembro.Estudiante &&
                                 m.Estatus == true);

            var gruposResponse = new List<GrupoResponse>();
            foreach (var grupo in grupos)
            {
                var totalMiembros = await _context.MiembrosCurso
                    .CountAsync(m => m.IdGrupo == grupo.IdGrupo && m.Estatus == true);

                gruposResponse.Add(new GrupoResponse
                {
                    IdGrupo = grupo.IdGrupo,
                    Nombre = grupo.Nombre,
                    Descripcion = grupo.Descripcion,
                    Estatus = grupo.Estatus,
                    TotalMiembros = totalMiembros
                });
            }

            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7120";

            return new CursoResponse
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
                TotalGrupos = gruposResponse.Count,
                Grupos = gruposResponse,
                Invitacion = new InvitacionCursoResponse
                {
                    Codigo = codigoInvitacion,
                    Token = tokenEnlace,
                    EnlaceInvitacion = tokenEnlace != null
                        ? $"{baseUrl}/api/cursos/unirse?token={tokenEnlace}"
                        : null
                }
            };
        }
    }
}