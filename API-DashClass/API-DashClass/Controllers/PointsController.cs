using Microsoft.AspNetCore.Mvc;
using API_DashClass.Services.Interfaces;
using API_DashClass.Models.Request;

namespace API_DashClass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PointsController : ControllerBase
    {
        private readonly IGamificationService _gamificationService;

        public PointsController(IGamificationService gamificationService)
        {
            _gamificationService = gamificationService;
        }

        /// <summary>
        /// Obtiene el balance de puntos de un usuario en un curso
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="courseId">ID del curso</param>
        /// <returns>Balance de puntos</returns>
        [HttpGet("balance/{userId}/{courseId}")]
        public async Task<IActionResult> GetBalance(int userId, int courseId)
        {
            try
            {
                var balance = await _gamificationService.ObtenerBalanceAsync(userId, courseId);

                if (balance == null)
                {
                    return NotFound(new { message = "Usuario o curso no encontrado" });
                }

                return Ok(balance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener balance", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el historial de transacciones de un usuario en un curso
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="courseId">ID del curso</param>
        /// <param name="limit">Número máximo de transacciones a retornar</param>
        /// <returns>Lista de transacciones</returns>
        [HttpGet("transactions/{userId}/{courseId}")]
        public async Task<IActionResult> GetTransactions(int userId, int courseId, [FromQuery] int limit = 50)
        {
            try
            {
                var transactions = await _gamificationService.ObtenerHistorialAsync(userId, courseId, limit);
                return Ok(new { total = transactions.Count, transactions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener transacciones", error = ex.Message });
            }
        }

        /// <summary>
        /// Registra puntos manualmente (solo profesores)
        /// </summary>
        /// <param name="request">Datos de los puntos a otorgar</param>
        /// <returns>Transacción creada</returns>
        [HttpPost("manual")]
        public async Task<IActionResult> AddManualPoints([FromBody] ManualPointsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var transaction = await _gamificationService.RegistrarPuntosManualAsync(request);
                return Ok(new
                {
                    message = "Puntos registrados exitosamente",
                    transaction
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al registrar puntos", error = ex.Message });
            }
        }

        /// <summary>
        /// Transfiere puntos de un estudiante a otro
        /// </summary>
        /// <param name="request">Datos de la transferencia</param>
        /// <returns>Información de la transferencia</returns>
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferPoints([FromBody] TransferPointsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var transfer = await _gamificationService.TransferirPuntosAsync(request);
                return Ok(new
                {
                    message = "Transferencia exitosa",
                    transfer
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al transferir puntos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el ranking de estudiantes por puntos en un curso
        /// </summary>
        /// <param name="courseId">ID del curso</param>
        /// <param name="top">Número de estudiantes a retornar</param>
        /// <returns>Ranking de estudiantes</returns>
        [HttpGet("ranking/{courseId}")]
        public async Task<IActionResult> GetRanking(int courseId, [FromQuery] int top = 10)
        {
            try
            {
                var ranking = await _gamificationService.ObtenerRankingAsync(courseId, top);
                return Ok(new { total = ranking.Count, ranking });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener ranking", error = ex.Message });
            }
        }
    }
}