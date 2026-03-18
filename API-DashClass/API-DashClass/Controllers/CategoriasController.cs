using API_DashClass.Models.Request;
using API_DashClass.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_DashClass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        /// <summary>
        /// Crea una nueva categoría en un curso
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var categoria = await _categoriaService.CrearCategoriaAsync(request);
                return Ok(new { message = "Categoría creada exitosamente", data = categoria });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al crear categoría", error = ex.Message }); }
        }

        /// <summary>
        /// Obtiene todas las categorías de un curso con su peso y total de actividades
        /// </summary>
        [HttpGet("curso/{idCurso}")]
        public async Task<IActionResult> ObtenerCategorias(int idCurso)
        {
            try
            {
                var categorias = await _categoriaService.ObtenerCategoriasCursoAsync(idCurso);
                var pesoTotal = categorias.Sum(c => c.Peso);
                return Ok(new { total = categorias.Count, pesoTotal, categorias });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener categorías", error = ex.Message }); }
        }

        /// <summary>
        /// Obtiene una categoría por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerCategoria(int id)
        {
            try
            {
                var categoria = await _categoriaService.ObtenerCategoriaPorIdAsync(id);
                if (categoria == null) return NotFound(new { message = "Categoría no encontrada" });
                return Ok(categoria);
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener categoría", error = ex.Message }); }
        }

        /// <summary>
        /// Actualiza nombre, peso y descripción de una categoría
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCategoria(int id, [FromBody] ActualizarCategoriaRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var categoria = await _categoriaService.ActualizarCategoriaAsync(id, request);
                return Ok(new { message = "Categoría actualizada exitosamente", data = categoria });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al actualizar categoría", error = ex.Message }); }
        }

        /// <summary>
        /// Elimina una categoría (las actividades quedan sin categoría)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            try
            {
                var resultado = await _categoriaService.EliminarCategoriaAsync(id);
                if (!resultado) return NotFound(new { message = "Categoría no encontrada" });
                return Ok(new { message = "Categoría eliminada. Las actividades asociadas quedaron sin categoría." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al eliminar categoría", error = ex.Message }); }
        }

        /// <summary>
        /// Calcula la calificación final ponderada de un estudiante en un curso
        /// </summary>
        [HttpGet("calificacion/usuario/{idUsuario}/curso/{idCurso}")]
        public async Task<IActionResult> CalificacionFinal(int idUsuario, int idCurso)
        {
            try
            {
                var resultado = await _categoriaService.CalcularCalificacionFinalAsync(idUsuario, idCurso);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al calcular calificación", error = ex.Message }); }
        }

        /// <summary>
        /// Calcula la calificación final de todos los estudiantes de un curso
        /// </summary>
        [HttpGet("calificacion/curso/{idCurso}")]
        public async Task<IActionResult> CalificacionesCurso(int idCurso)
        {
            try
            {
                var resultados = await _categoriaService.CalcularCalificacionesCursoAsync(idCurso);
                return Ok(new { total = resultados.Count, calificaciones = resultados });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al calcular calificaciones", error = ex.Message }); }
        }
    }
}