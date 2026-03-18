using API_DashClass.Models.Request;
using API_DashClass.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_DashClass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActividadesController : ControllerBase
    {
        private readonly IActividadService _actividadService;

        public ActividadesController(IActividadService actividadService)
        {
            _actividadService = actividadService;
        }

        /// <summary>
        /// Crea una nueva actividad y la asigna a grupos del curso
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearActividad([FromBody] CrearActividadRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var actividad = await _actividadService.CrearActividadAsync(request);
                return Ok(new { message = "Actividad creada exitosamente", data = actividad });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al crear actividad", error = ex.Message }); }
        }

        /// <summary>
        /// Obtiene todas las actividades de un curso (opcionalmente filtradas por estatus)
        /// </summary>
        [HttpGet("curso/{idCurso}")]
        public async Task<IActionResult> ObtenerActividadesCurso(
            int idCurso, [FromQuery] string? estatus = null)
        {
            try
            {
                var actividades = await _actividadService.ObtenerActividadesCursoAsync(idCurso, estatus);
                return Ok(new { total = actividades.Count, actividades });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener actividades", error = ex.Message }); }
        }

        /// <summary>
        /// Obtiene actividades de un curso filtradas por grupo
        /// </summary>
        [HttpGet("curso/{idCurso}/grupo/{idGrupo}")]
        public async Task<IActionResult> ObtenerActividadesGrupo(
            int idCurso, int idGrupo, [FromQuery] string? estatus = null)
        {
            try
            {
                var actividades = await _actividadService.ObtenerActividadesGrupoAsync(idCurso, idGrupo, estatus);
                return Ok(new { total = actividades.Count, actividades });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener actividades del grupo", error = ex.Message }); }
        }

        /// <summary>
        /// Obtiene una actividad por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerActividad(int id)
        {
            try
            {
                var actividad = await _actividadService.ObtenerActividadPorIdAsync(id);
                if (actividad == null) return NotFound(new { message = "Actividad no encontrada" });
                return Ok(actividad);
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener actividad", error = ex.Message }); }
        }

        /// <summary>
        /// Actualiza una actividad existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarActividad(
            int id, [FromBody] ActualizarActividadRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var actividad = await _actividadService.ActualizarActividadAsync(id, request);
                return Ok(new { message = "Actividad actualizada exitosamente", data = actividad });
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al actualizar actividad", error = ex.Message }); }
        }

        /// <summary>
        /// Cambia el estatus de una actividad (Borrador, Publicado, Programado, Archivado)
        /// </summary>
        [HttpPatch("{id}/estatus")]
        public async Task<IActionResult> CambiarEstatus(int id, [FromBody] string estatus)
        {
            try
            {
                var resultado = await _actividadService.CambiarEstatusActividadAsync(id, estatus);
                if (!resultado) return NotFound(new { message = "Actividad no encontrada" });
                return Ok(new { message = $"Estatus cambiado a {estatus}" });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al cambiar estatus", error = ex.Message }); }
        }
    }
}