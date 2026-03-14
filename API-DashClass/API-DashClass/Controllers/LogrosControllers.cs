using Microsoft.AspNetCore.Mvc;
using API_DashClass.Services.Interfaces;
using API_DashClass.Models.Request;

namespace API_DashClass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogrosController : ControllerBase
    {
        private readonly ILogroService _logroService;

        public LogrosController(ILogroService logroService)
        {
            _logroService = logroService;
        }

        /// <summary>
        /// Crea un nuevo logro
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearLogro([FromBody] CrearLogroRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var logro = await _logroService.CrearLogroAsync(request);
                return Ok(new
                {
                    message = "Logro creado exitosamente",
                    logro
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear logro", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todos los logros de un curso
        /// </summary>
        [HttpGet("curso/{cursoId}")]
        public async Task<IActionResult> ObtenerLogrosCurso(int cursoId)
        {
            try
            {
                var logros = await _logroService.ObtenerLogrosCursoAsync(cursoId);
                return Ok(new
                {
                    total = logros.Count,
                    logros
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener logros", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todos los logros activos de un curso
        /// </summary>
        [HttpGet("curso/{cursoId}/activos")]
        public async Task<IActionResult> ObtenerLogrosActivos(int cursoId)
        {
            try
            {
                var logros = await _logroService.ObtenerLogrosActivosAsync(cursoId);
                return Ok(new
                {
                    total = logros.Count,
                    logros
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener logros", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los detalles de un logro específico
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerLogro(int id)
        {
            try
            {
                var logro = await _logroService.ObtenerLogroPorIdAsync(id);

                if (logro == null)
                {
                    return NotFound(new { message = "Logro no encontrado" });
                }

                return Ok(logro);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener logro", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un logro existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarLogro(int id, [FromBody] ActualizarLogroRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var logro = await _logroService.ActualizarLogroAsync(id, request);
                return Ok(new
                {
                    message = "Logro actualizado exitosamente",
                    logro
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar logro", error = ex.Message });
            }
        }

        /// <summary>
        /// Cambia el estatus de un logro (activar/desactivar)
        /// </summary>
        [HttpPatch("{id}/estatus")]
        public async Task<IActionResult> CambiarEstatus(int id, [FromBody] bool estatus)
        {
            try
            {
                var resultado = await _logroService.CambiarEstatusLogroAsync(id, estatus);

                if (!resultado)
                {
                    return NotFound(new { message = "Logro no encontrado" });
                }

                return Ok(new
                {
                    message = estatus ? "Logro activado" : "Logro desactivado"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al cambiar estatus", error = ex.Message });
            }
        }

        /// <summary>
        /// Desbloquea un logro para un usuario
        /// </summary>
        [HttpPost("desbloquear")]
        public async Task<IActionResult> DesbloquearLogro([FromBody] DesbloquearLogroRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var logroUsuario = await _logroService.DesbloquearLogroAsync(request);
                return Ok(new
                {
                    message = "Logro desbloqueado exitosamente",
                    logroUsuario
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al desbloquear logro", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los logros desbloqueados de un usuario en un curso
        /// </summary>
        [HttpGet("usuario/{userId}/curso/{courseId}")]
        public async Task<IActionResult> ObtenerLogrosUsuario(int userId, int courseId)
        {
            try
            {
                var logros = await _logroService.ObtenerLogrosUsuarioAsync(userId, courseId);
                return Ok(new
                {
                    total = logros.Count,
                    logros
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener logros del usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el progreso de logros de un usuario
        /// </summary>
        [HttpGet("progreso/usuario/{userId}/curso/{courseId}")]
        public async Task<IActionResult> ObtenerProgresoLogros(int userId, int courseId)
        {
            try
            {
                var progreso = await _logroService.ObtenerProgresoLogrosAsync(userId, courseId);
                return Ok(progreso);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener progreso de logros", error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si un usuario tiene un logro específico
        /// </summary>
        [HttpGet("verificar/usuario/{userId}/logro/{logroId}")]
        public async Task<IActionResult> VerificarLogro(int userId, int logroId)
        {
            try
            {
                var tieneLogro = await _logroService.UsuarioTieneLogroAsync(userId, logroId);
                return Ok(new
                {
                    tieneLogro
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al verificar logro", error = ex.Message });
            }
        }
    }
}