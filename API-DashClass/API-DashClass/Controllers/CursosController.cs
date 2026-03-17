using API_DashClass.Models.Request;
using API_DashClass.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_DashClass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CursosController : ControllerBase
    {
        private readonly ICursoService _cursoService;

        public CursosController(ICursoService cursoService)
        {
            _cursoService = cursoService;
        }

        /// <summary>
        /// Crea un nuevo curso con grupos iniciales
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearCurso([FromBody] CrearCursoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var curso = await _cursoService.CrearCursoAsync(request);
                return Ok(new { message = "Curso creado exitosamente", data = curso });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear curso", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un curso por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerCurso(int id)
        {
            try
            {
                var curso = await _cursoService.ObtenerCursoPorIdAsync(id);

                if (curso == null)
                    return NotFound(new { message = "Curso no encontrado" });

                return Ok(curso);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener curso", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todos los cursos de un usuario
        /// </summary>
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> ObtenerCursosDeUsuario(int idUsuario)
        {
            try
            {
                var cursos = await _cursoService.ObtenerCursosDeUsuarioAsync(idUsuario);
                return Ok(new { total = cursos.Count, cursos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener cursos", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza los datos de un curso
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCurso(int id, [FromBody] ActualizarCursoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var curso = await _cursoService.ActualizarCursoAsync(id, request);
                return Ok(new { message = "Curso actualizado exitosamente", data = curso });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar curso", error = ex.Message });
            }
        }

        /// <summary>
        /// Activa o desactiva un curso
        /// </summary>
        [HttpPatch("{id}/estatus")]
        public async Task<IActionResult> CambiarEstatus(int id, [FromBody] bool estatus)
        {
            try
            {
                var resultado = await _cursoService.CambiarEstatusCursoAsync(id, estatus);

                if (!resultado)
                    return NotFound(new { message = "Curso no encontrado" });

                return Ok(new { message = estatus ? "Curso activado" : "Curso desactivado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al cambiar estatus", error = ex.Message });
            }
        }

        /// <summary>
        /// Une a un usuario a un curso con código o enlace
        /// </summary>
        [HttpPost("unirse")]
        public async Task<IActionResult> UnirseACurso([FromBody] UnirseACursoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var curso = await _cursoService.UnirseACursoAsync(request);
                return Ok(new { message = "Te has unido al curso exitosamente", data = curso });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al unirse al curso", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los miembros de un curso
        /// </summary>
        [HttpGet("{id}/miembros")]
        public async Task<IActionResult> ObtenerMiembros(int id)
        {
            try
            {
                var miembros = await _cursoService.ObtenerMiembrosCursoAsync(id);
                return Ok(new { total = miembros.Count, miembros });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener miembros", error = ex.Message });
            }
        }
    }
}