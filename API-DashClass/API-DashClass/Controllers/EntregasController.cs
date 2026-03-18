using API_DashClass.Models.Request;
using API_DashClass.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_DashClass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntregasController : ControllerBase
    {
        private readonly IEntregaService _entregaService;

        public EntregasController(IEntregaService entregaService)
        {
            _entregaService = entregaService;
        }

        /// <summary>
        /// Marcar actividad como entregada sin archivos
        /// </summary>
        [HttpPost("marcar")]
        public async Task<IActionResult> MarcarEntregada([FromBody] MarcarEntregadaRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var entrega = await _entregaService.MarcarEntregadaAsync(request);
                return Ok(new { message = "Actividad marcada como entregada", data = entrega });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al registrar entrega", error = ex.Message }); }
        }

        /// <summary>
        /// Subir entrega con archivos (multipart/form-data)
        /// </summary>
        [HttpPost("subir")]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB máximo
        public async Task<IActionResult> SubirEntrega([FromForm] SubirEntregaRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var entrega = await _entregaService.SubirEntregaAsync(request);
                return Ok(new { message = "Entrega subida exitosamente", data = entrega });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al subir entrega", error = ex.Message }); }
        }

        /// <summary>
        /// Obtiene la entrega activa de un estudiante en una actividad
        /// </summary>
        [HttpGet("actividad/{idActividad}/estudiante/{idUsuario}")]
        public async Task<IActionResult> ObtenerEntregaEstudiante(int idActividad, int idUsuario)
        {
            try
            {
                var entrega = await _entregaService.ObtenerEntregaEstudianteAsync(idActividad, idUsuario);
                if (entrega == null) return NotFound(new { message = "Sin entrega" });
                return Ok(entrega);
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener entrega", error = ex.Message }); }
        }

        /// <summary>
        /// Obtiene todas las entregas de una actividad (para el profesor)
        /// </summary>
        [HttpGet("actividad/{idActividad}")]
        public async Task<IActionResult> ObtenerEntregasActividad(int idActividad)
        {
            try
            {
                var entregas = await _entregaService.ObtenerEntregasActividadAsync(idActividad);
                return Ok(new { total = entregas.Count, entregas });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener entregas", error = ex.Message }); }
        }

        /// <summary>
        /// Califica una entrega
        /// </summary>
        [HttpPost("{idEntrega}/calificar")]
        public async Task<IActionResult> Calificar(int idEntrega, [FromBody] CalificarEntregaRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var calificacion = await _entregaService.CalificarEntregaAsync(idEntrega, request);
                return Ok(new { message = "Entrega calificada exitosamente", data = calificacion });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al calificar", error = ex.Message }); }
        }

        /// <summary>
        /// Obtiene la calificación de una entrega
        /// </summary>
        [HttpGet("{idEntrega}/calificacion")]
        public async Task<IActionResult> ObtenerCalificacion(int idEntrega)
        {
            try
            {
                var calificacion = await _entregaService.ObtenerCalificacionAsync(idEntrega);
                if (calificacion == null) return NotFound(new { message = "Sin calificación" });
                return Ok(calificacion);
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener calificación", error = ex.Message }); }
        }
    }
}