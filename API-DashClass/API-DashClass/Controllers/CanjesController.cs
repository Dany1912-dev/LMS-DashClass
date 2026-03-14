using Microsoft.AspNetCore.Mvc;
using API_DashClass.Services.Interfaces;
using API_DashClass.Models.Request;

namespace API_DashClass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CanjesController : ControllerBase
    {
        private readonly ICanjeService _canjeService;

        public CanjesController(ICanjeService canjeService)
        {
            _canjeService = canjeService;
        }

        /// <summary>
        /// Canjea una recompensa (gasta puntos del estudiante)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CanjearRecompensa([FromBody] CanjearRecompensaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var canje = await _canjeService.CanjearRecompensaAsync(request);
                return Ok(new
                {
                    message = "Recompensa canjeada exitosamente",
                    canje
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al canjear recompensa", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los canjes de un estudiante en un curso
        /// </summary>
        [HttpGet("estudiante/{userId}/curso/{courseId}")]
        public async Task<IActionResult> ObtenerCanjesEstudiante(int userId, int courseId)
        {
            try
            {
                var canjes = await _canjeService.ObtenerCanjesEstudianteAsync(userId, courseId);
                return Ok(new
                {
                    total = canjes.Count,
                    canjes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener canjes", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todos los canjes de un curso (para profesor)
        /// </summary>
        [HttpGet("curso/{courseId}")]
        public async Task<IActionResult> ObtenerCanjesCurso(int courseId, [FromQuery] string? estado = null)
        {
            try
            {
                var canjes = await _canjeService.ObtenerCanjesCursoAsync(courseId, estado);
                return Ok(new
                {
                    total = canjes.Count,
                    canjes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener canjes", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los detalles de un canje específico
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerCanje(int id)
        {
            try
            {
                var canje = await _canjeService.ObtenerCanjePorIdAsync(id);

                if (canje == null)
                {
                    return NotFound(new { message = "Canje no encontrado" });
                }

                return Ok(canje);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener canje", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un canje por su código
        /// </summary>
        [HttpGet("codigo/{codigoCanje}")]
        public async Task<IActionResult> ObtenerCanjePorCodigo(string codigoCanje)
        {
            try
            {
                var canje = await _canjeService.ObtenerCanjePorCodigoAsync(codigoCanje);

                if (canje == null)
                {
                    return NotFound(new { message = "Código de canje no válido" });
                }

                return Ok(canje);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener canje", error = ex.Message });
            }
        }

        /// <summary>
        /// Reclama un canje (profesor valida código)
        /// </summary>
        [HttpPost("reclamar/{codigoCanje}")]
        public async Task<IActionResult> ReclamarCanje(string codigoCanje)
        {
            try
            {
                var canje = await _canjeService.ReclamarCanjeAsync(codigoCanje);
                return Ok(new
                {
                    message = "Canje reclamado exitosamente",
                    canje
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al reclamar canje", error = ex.Message });
            }
        }

        /// <summary>
        /// Cancela un canje y devuelve los puntos al estudiante
        /// </summary>
        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> CancelarCanje(int id, [FromBody] int idUsuarioProfesor)
        {
            try
            {
                var resultado = await _canjeService.CancelarCanjeAsync(id, idUsuarioProfesor);

                if (!resultado)
                {
                    return NotFound(new { message = "Canje no encontrado" });
                }

                return Ok(new
                {
                    message = "Canje cancelado y puntos devueltos exitosamente"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al cancelar canje", error = ex.Message });
            }
        }
    }
}