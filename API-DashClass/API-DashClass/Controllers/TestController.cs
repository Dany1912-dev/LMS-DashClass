using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_DashClass.Data;

namespace API_DashClass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("database")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                // Intentar conectar a la base de datos
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    // Contar usuarios
                    var userCount = await _context.Usuarios.CountAsync();
                    
                    return Ok(new
                    {
                        message = "✅ Conexión exitosa a la base de datos",
                        database = "lms_gamificacion",
                        totalUsuarios = userCount,
                        timestamp = DateTime.Now
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        message = "❌ No se pudo conectar a la base de datos"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "❌ Error al conectar",
                    error = ex.Message
                });
            }
        }
    }
}