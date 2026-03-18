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

        public async Task<CursoResponse> CrearCursoAsync(CrearCursoRequest request)
        {
            if (request.Grupos == null || request.Grupos.Count == 0)
                throw new InvalidOperationException("Debes crear al menos un grupo");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == request.IdUsuario)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            var codigo = await GenerarCodigoUnicoAsync();

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

            foreach (var grupoRequest in request.Grupos)
            {
                await CrearGrupoConInvitacionAsync(nuevoCurso.IdCurso, grupoRequest, DateTime.UtcNow.AddDays(7));
            }

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

            return await ObtenerCursoPorIdAsync(nuevoCurso.IdCurso)
                ?? throw new InvalidOperationException("Error al obtener el curso creado");
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

            var invitaciones = await _context.InvitacionesCurso
                .Where(i => i.IdCurso == idCurso && i.Estatus == true)
                .ToListAsync();

            return await MapearCursoAResponseAsync(curso, profesor, grupos, invitaciones);
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
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == request.IdUsuario)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            var invitacion = await _context.InvitacionesCurso
                .FirstOrDefaultAsync(i =>
                    (i.Codigo == request.CodigoOToken || i.Token == request.CodigoOToken) &&
                    i.Estatus == true);

            if (invitacion == null)
                throw new InvalidOperationException("Código o enlace inválido");

            if (invitacion.FechaExpiracion.HasValue && invitacion.FechaExpiracion < DateTime.UtcNow)
                throw new InvalidOperationException("La invitación ha expirado");

            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == invitacion.IdCurso && c.Estatus == true)
                ?? throw new InvalidOperationException("El curso no está disponible");

            var yaEsMiembro = await _context.MiembrosCurso
                .AnyAsync(m => m.IdCurso == curso.IdCurso &&
                               m.IdUsuario == request.IdUsuario &&
                               m.Estatus == true);

            if (yaEsMiembro)
                throw new InvalidOperationException("Ya eres miembro de este curso");

            _context.MiembrosCurso.Add(new MiembrosCursos
            {
                IdCurso = curso.IdCurso,
                IdUsuario = request.IdUsuario,
                IdGrupo = invitacion.IdGrupo,
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
        // CREAR INVITACIÓN
        // ========================================

        public async Task<InvitacionCursoResponse> CrearInvitacionAsync(int idCurso, CrearInvitacionRequest request)
        {
            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == idCurso)
                ?? throw new InvalidOperationException("Curso no encontrado");

            var grupo = await _context.Grupos
                .FirstOrDefaultAsync(g => g.IdGrupo == request.IdGrupo && g.IdCurso == idCurso)
                ?? throw new InvalidOperationException("El grupo no existe en este curso");

            DateTime? fechaExpiracion = request.Duracion switch
            {
                DuracionInvitacion.UnDia => DateTime.UtcNow.AddDays(1),
                DuracionInvitacion.UnaSemana => DateTime.UtcNow.AddDays(7),
                DuracionInvitacion.UnMes => DateTime.UtcNow.AddMonths(1),
                DuracionInvitacion.SinExpiracion => null,
                _ => DateTime.UtcNow.AddDays(7)
            };

            var codigoInvitacion = GenerarCodigoInvitacion();
            var tokenEnlace = GenerarTokenEnlace();

            _context.InvitacionesCurso.Add(new InvitacionesCurso
            {
                IdCurso = idCurso,
                IdGrupo = request.IdGrupo,
                Tipo = InvitacionesCurso.TipoInvitacion.Codigo,
                Codigo = codigoInvitacion,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = fechaExpiracion,
                Estatus = true
            });

            _context.InvitacionesCurso.Add(new InvitacionesCurso
            {
                IdCurso = idCurso,
                IdGrupo = request.IdGrupo,
                Tipo = InvitacionesCurso.TipoInvitacion.Enlace,
                Token = tokenEnlace,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = fechaExpiracion,
                Estatus = true
            });

            await _context.SaveChangesAsync();

            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7120";

            return new InvitacionCursoResponse
            {
                Codigo = codigoInvitacion,
                Token = tokenEnlace,
                EnlaceInvitacion = $"{baseUrl}/api/cursos/unirse?token={tokenEnlace}",
                NombreGrupo = grupo.Nombre,
                FechaExpiracion = fechaExpiracion
            };
        }

        // ========================================
        // OBTENER INVITACIONES
        // ========================================

        public async Task<List<InvitacionCursoResponse>> ObtenerInvitacionesAsync(int idCurso)
        {
            var invitaciones = await _context.InvitacionesCurso
                .Where(i => i.IdCurso == idCurso && i.Estatus == true &&
                            i.Tipo == InvitacionesCurso.TipoInvitacion.Codigo)
                .ToListAsync();

            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7120";
            var responses = new List<InvitacionCursoResponse>();

            foreach (var inv in invitaciones)
            {
                var grupo = inv.IdGrupo.HasValue
                    ? await _context.Grupos.FirstOrDefaultAsync(g => g.IdGrupo == inv.IdGrupo)
                    : null;

                var tokenEnlace = await _context.InvitacionesCurso
                    .Where(i => i.IdCurso == idCurso &&
                                i.IdGrupo == inv.IdGrupo &&
                                i.Tipo == InvitacionesCurso.TipoInvitacion.Enlace &&
                                i.Estatus == true)
                    .Select(i => i.Token)
                    .FirstOrDefaultAsync();

                responses.Add(new InvitacionCursoResponse
                {
                    Codigo = inv.Codigo,
                    Token = tokenEnlace,
                    EnlaceInvitacion = tokenEnlace != null
                        ? $"{baseUrl}/api/cursos/unirse?token={tokenEnlace}"
                        : null,
                    NombreGrupo = grupo?.Nombre,
                    FechaExpiracion = inv.FechaExpiracion
                });
            }

            return responses;
        }

        // ========================================
        // AGREGAR GRUPO
        // ========================================

        public async Task<GrupoResponse> AgregarGrupoAsync(int idCurso, CrearGrupoRequest request)
        {
            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == idCurso)
                ?? throw new InvalidOperationException("Curso no encontrado");

            var grupo = await CrearGrupoConInvitacionAsync(idCurso, request, DateTime.UtcNow.AddDays(7));

            var invCodigo = await _context.InvitacionesCurso
                .FirstOrDefaultAsync(i => i.IdGrupo == grupo.IdGrupo &&
                                          i.Tipo == InvitacionesCurso.TipoInvitacion.Codigo);

            var invEnlace = await _context.InvitacionesCurso
                .FirstOrDefaultAsync(i => i.IdGrupo == grupo.IdGrupo &&
                                          i.Tipo == InvitacionesCurso.TipoInvitacion.Enlace);

            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7120";

            return new GrupoResponse
            {
                IdGrupo = grupo.IdGrupo,
                Nombre = grupo.Nombre,
                Descripcion = grupo.Descripcion,
                Estatus = grupo.Estatus,
                TotalMiembros = 0,
                Invitacion = new InvitacionCursoResponse
                {
                    Codigo = invCodigo?.Codigo,
                    Token = invEnlace?.Token,
                    EnlaceInvitacion = invEnlace?.Token != null
                        ? $"{baseUrl}/api/cursos/unirse?token={invEnlace.Token}"
                        : null,
                    NombreGrupo = grupo.Nombre,
                    FechaExpiracion = invCodigo?.FechaExpiracion
                }
            };
        }

        // ========================================
        // OBTENER GRUPOS POR CURSO
        // ========================================

        public async Task<List<GrupoResponse>> ObtenerGruposPorCursoAsync(int idCurso)
        {
            var cursoExiste = await _context.Cursos.AnyAsync(c => c.IdCurso == idCurso);
            if (!cursoExiste)
                throw new InvalidOperationException("Curso no encontrado");

            var grupos = await _context.Grupos
                .Where(g => g.IdCurso == idCurso && g.Estatus == true)
                .ToListAsync();

            var invitaciones = await _context.InvitacionesCurso
                .Where(i => i.IdCurso == idCurso && i.Estatus == true)
                .ToListAsync();

            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7120";
            var responses = new List<GrupoResponse>();

            foreach (var grupo in grupos)
            {
                var totalMiembros = await _context.MiembrosCurso
                    .CountAsync(m => m.IdGrupo == grupo.IdGrupo && m.Estatus == true);

                var invCodigo = invitaciones.FirstOrDefault(i =>
                    i.IdGrupo == grupo.IdGrupo &&
                    i.Tipo == InvitacionesCurso.TipoInvitacion.Codigo);

                var invEnlace = invitaciones.FirstOrDefault(i =>
                    i.IdGrupo == grupo.IdGrupo &&
                    i.Tipo == InvitacionesCurso.TipoInvitacion.Enlace);

                responses.Add(new GrupoResponse
                {
                    IdGrupo = grupo.IdGrupo,
                    Nombre = grupo.Nombre,
                    Descripcion = grupo.Descripcion,
                    Estatus = grupo.Estatus,
                    TotalMiembros = totalMiembros,
                    Invitacion = new InvitacionCursoResponse
                    {
                        Codigo = invCodigo?.Codigo,
                        Token = invEnlace?.Token,
                        EnlaceInvitacion = invEnlace?.Token != null
                            ? $"{baseUrl}/api/cursos/unirse?token={invEnlace.Token}"
                            : null,
                        NombreGrupo = grupo.Nombre,
                        FechaExpiracion = invCodigo?.FechaExpiracion
                    }
                });
            }

            return responses;
        }

        // ========================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ========================================

        private async Task<Grupos> CrearGrupoConInvitacionAsync(int idCurso, CrearGrupoRequest request, DateTime? fechaExpiracion)
        {
            var grupo = new Grupos
            {
                IdCurso = idCurso,
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                FechaCreacion = DateTime.UtcNow,
                Estatus = true
            };

            _context.Grupos.Add(grupo);
            await _context.SaveChangesAsync();

            _context.InvitacionesCurso.Add(new InvitacionesCurso
            {
                IdCurso = idCurso,
                IdGrupo = grupo.IdGrupo,
                Tipo = InvitacionesCurso.TipoInvitacion.Codigo,
                Codigo = GenerarCodigoInvitacion(),
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = fechaExpiracion,
                Estatus = true
            });

            _context.InvitacionesCurso.Add(new InvitacionesCurso
            {
                IdCurso = idCurso,
                IdGrupo = grupo.IdGrupo,
                Tipo = InvitacionesCurso.TipoInvitacion.Enlace,
                Token = GenerarTokenEnlace(),
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = fechaExpiracion,
                Estatus = true
            });

            await _context.SaveChangesAsync();
            return grupo;
        }

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

        private static string GenerarCodigoInvitacion()
        {
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }

        private static string GenerarTokenEnlace()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private static string GenerarStringAleatorio(int longitud)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var resultado = new char[longitud];
            for (int i = 0; i < longitud; i++)
                resultado[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            return new string(resultado);
        }

        private async Task<CursoResponse> MapearCursoAResponseAsync(
            Cursos curso, Usuario? profesor, List<Grupos> grupos, List<InvitacionesCurso> invitaciones)
        {
            var totalEstudiantes = await _context.MiembrosCurso
                .CountAsync(m => m.IdCurso == curso.IdCurso &&
                                 m.Rol == MiembrosCursos.RolMiembro.Estudiante &&
                                 m.Estatus == true);

            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7120";
            var gruposResponse = new List<GrupoResponse>();

            foreach (var grupo in grupos)
            {
                var totalMiembros = await _context.MiembrosCurso
                    .CountAsync(m => m.IdGrupo == grupo.IdGrupo && m.Estatus == true);

                var invCodigo = invitaciones.FirstOrDefault(i =>
                    i.IdGrupo == grupo.IdGrupo &&
                    i.Tipo == InvitacionesCurso.TipoInvitacion.Codigo);

                var invEnlace = invitaciones.FirstOrDefault(i =>
                    i.IdGrupo == grupo.IdGrupo &&
                    i.Tipo == InvitacionesCurso.TipoInvitacion.Enlace);

                gruposResponse.Add(new GrupoResponse
                {
                    IdGrupo = grupo.IdGrupo,
                    Nombre = grupo.Nombre,
                    Descripcion = grupo.Descripcion,
                    Estatus = grupo.Estatus,
                    TotalMiembros = totalMiembros,
                    Invitacion = new InvitacionCursoResponse
                    {
                        Codigo = invCodigo?.Codigo,
                        Token = invEnlace?.Token,
                        EnlaceInvitacion = invEnlace?.Token != null
                            ? $"{baseUrl}/api/cursos/unirse?token={invEnlace.Token}"
                            : null,
                        NombreGrupo = grupo.Nombre,
                        FechaExpiracion = invCodigo?.FechaExpiracion
                    }
                });
            }

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
                Grupos = gruposResponse
            };
        }
    }
}