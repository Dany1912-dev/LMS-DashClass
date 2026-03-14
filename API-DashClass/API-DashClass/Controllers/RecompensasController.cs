using Microsoft.AspNetCore.Mvc;
using API_DashClass.Services.Interfaces;
using API_DashClass.Models.Request;

namespace API_DashClass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecompensasController : ControllerBase
    {
        private readonly IRecompensaService _recompensaService;

        public RecompensasController(IRecompensaService recompensaService)
        {
            _recompensaService = recompensaService;
        }

        /// <summary>
        /// Crea una nueva recompensa
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearRecompensa([FromBody] CrearRecompensaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var recompensa = await _recompensaService.CrearRecompensaAsync(request);
                return Ok(new
                {
                    message = "Recompensa creada exitosamente",
                    recompensa
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear recompensa", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todas las recompensas activas de un curso
        /// </summary>
        [HttpGet("curso/{cursoId}/activas")]
        public async Task<IActionResult> ObtenerRecompensasActivas(int cursoId)
        {
            try
            {
                var recompensas = await _recompensaService.ObtenerRecompensasActivasAsync(cursoId);
                return Ok(new
                {
                    total = recompensas.Count,
                    recompensas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener recompensas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todas las recompensas de un curso (para profesores)
        /// </summary>
        [HttpGet("curso/{cursoId}/todas")]
        public async Task<IActionResult> ObtenerTodasRecompensas(int cursoId)
        {
            try
            {
                var recompensas = await _recompensaService.ObtenerTodasRecompensasAsync(cursoId);
                return Ok(new
                {
                    total = recompensas.Count,
                    recompensas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener recompensas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los detalles de una recompensa específica
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerRecompensa(int id)
        {
            try
            {
                var recompensa = await _recompensaService.ObtenerRecompensaPorIdAsync(id);

                if (recompensa == null)
                {
                    return NotFound(new { message = "Recompensa no encontrada" });
                }

                return Ok(recompensa);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener recompensa", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una recompensa existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarRecompensa(int id, [FromBody] ActualizarRecompensaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var recompensa = await _recompensaService.ActualizarRecompensaAsync(id, request);
                return Ok(new
                {
                    message = "Recompensa actualizada exitosamente",
                    recompensa
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar recompensa", error = ex.Message });
            }
        }

        /// <summary>
        /// Cambia el estatus de una recompensa (activar/desactivar)
        /// </summary>
        [HttpPatch("{id}/estatus")]
        public async Task<IActionResult> CambiarEstatus(int id, [FromBody] bool estatus)
        {
            try
            {
                var resultado = await _recompensaService.CambiarEstatusRecompensaAsync(id, estatus);

                if (!resultado)
                {
                    return NotFound(new { message = "Recompensa no encontrada" });
                }

                return Ok(new
                {
                    message = estatus ? "Recompensa activada" : "Recompensa desactivada"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al cambiar estatus", error = ex.Message });
            }
        }

        /// <summary>
        /// Cambia el destacado de una recompensa
        /// </summary>
        [HttpPatch("{id}/destacado")]
        public async Task<IActionResult> CambiarDestacado(int id, [FromBody] bool destacado)
        {
            try
            {
                var resultado = await _recompensaService.CambiarDestacadoRecompensaAsync(id, destacado);

                if (!resultado)
                {
                    return NotFound(new { message = "Recompensa no encontrada" });
                }

                return Ok(new
                {
                    message = destacado ? "Recompensa destacada" : "Recompensa no destacada"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al cambiar destacado", error = ex.Message });
            }
        }

        /// <summary>
        /// Agrega stock a una recompensa
        /// </summary>
        [HttpPost("{id}/stock/agregar")]
        public async Task<IActionResult> AgregarStock(int id, [FromBody] int cantidad)
        {
            try
            {
                var resultado = await _recompensaService.AgregarStockAsync(id, cantidad);

                if (!resultado)
                {
                    return NotFound(new { message = "Recompensa no encontrada" });
                }

                return Ok(new
                {
                    message = $"Stock incrementado en {cantidad} unidades"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al agregar stock", error = ex.Message });
            }
        }
    }
}