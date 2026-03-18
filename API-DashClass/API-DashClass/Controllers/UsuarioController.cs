using API_DashClass.Models.Request;
using API_DashClass.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_DashClass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        /// <summary>
        /// Obtiene el perfil completo de un usuario
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPerfil(int id)
        {
            try
            {
                var usuario = await _usuarioService.ObtenerPerfilAsync(id);
                return Ok(usuario);
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener perfil", error = ex.Message }); }
        }

        /// <summary>
        /// Actualiza nombre, apellidos y biografía
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPerfil(int id, [FromBody] ActualizarPerfilRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var usuario = await _usuarioService.ActualizarPerfilAsync(id, request);
                return Ok(new { message = "Perfil actualizado exitosamente", data = usuario });
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al actualizar perfil", error = ex.Message }); }
        }

        /// <summary>
        /// Actualiza la foto de perfil
        /// </summary>
        [HttpPatch("{id}/foto")]
        public async Task<IActionResult> ActualizarFoto(int id, [FromBody] ActualizarFotoRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var usuario = await _usuarioService.ActualizarFotoAsync(id, request);
                return Ok(new { message = "Foto actualizada exitosamente", data = usuario });
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al actualizar foto", error = ex.Message }); }
        }

        /// <summary>
        /// Actualiza la biografía
        /// </summary>
        [HttpPatch("{id}/biografia")]
        public async Task<IActionResult> ActualizarBiografia(int id, [FromBody] string biografia)
        {
            try
            {
                var usuario = await _usuarioService.ActualizarBiografiaAsync(id, biografia);
                return Ok(new { message = "Biografía actualizada exitosamente", data = usuario });
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al actualizar biografía", error = ex.Message }); }
        }

        /// <summary>
        /// Activa o desactiva la cuenta
        /// </summary>
        [HttpPatch("{id}/estatus")]
        public async Task<IActionResult> CambiarEstatus(int id, [FromBody] bool estatus)
        {
            try
            {
                var resultado = await _usuarioService.CambiarEstatusAsync(id, estatus);
                if (!resultado) return NotFound(new { message = "Usuario no encontrado" });
                return Ok(new { message = estatus ? "Cuenta activada" : "Cuenta desactivada" });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al cambiar estatus", error = ex.Message }); }
        }

        /// <summary>
        /// Obtiene los cursos del usuario
        /// </summary>
        [HttpGet("{id}/cursos")]
        public async Task<IActionResult> ObtenerCursos(int id)
        {
            try
            {
                var cursos = await _usuarioService.ObtenerCursosAsync(id);
                return Ok(new { total = cursos.Count, cursos });
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener cursos", error = ex.Message }); }
        }

        /// <summary>
        /// Obtiene los logros desbloqueados del usuario
        /// </summary>
        [HttpGet("{id}/logros")]
        public async Task<IActionResult> ObtenerLogros(int id)
        {
            try
            {
                var logros = await _usuarioService.ObtenerLogrosAsync(id);
                return Ok(new { total = logros.Count, logros });
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener logros", error = ex.Message }); }
        }

        /// <summary>
        /// Obtiene las estadísticas del usuario
        /// </summary>
        [HttpGet("{id}/estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas(int id)
        {
            try
            {
                var estadisticas = await _usuarioService.ObtenerEstadisticasAsync(id);
                return Ok(estadisticas);
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener estadísticas", error = ex.Message }); }
        }

        /// <summary>
        /// Obtiene los métodos de autenticación vinculados
        /// </summary>
        [HttpGet("{id}/metodos-auth")]
        public async Task<IActionResult> ObtenerMetodosAuth(int id)
        {
            try
            {
                var metodos = await _usuarioService.ObtenerMetodosAuthAsync(id);
                return Ok(new { total = metodos.Count, metodos });
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener métodos de auth", error = ex.Message }); }
        }

        /// <summary>
        /// Vincula Google a la cuenta del usuario
        /// </summary>
        [HttpPost("{id}/metodos-auth/google")]
        public async Task<IActionResult> VincularGoogle(int id, [FromBody] VincularGoogleRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var metodo = await _usuarioService.VincularGoogleAsync(id, request);
                return Ok(new { message = "Google vinculado exitosamente", data = metodo });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al vincular Google", error = ex.Message }); }
        }

        /// <summary>
        /// Vincula Microsoft a la cuenta del usuario
        /// </summary>
        [HttpPost("{id}/metodos-auth/microsoft")]
        public async Task<IActionResult> VincularMicrosoft(int id, [FromBody] VincularMicrosoftRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var metodo = await _usuarioService.VincularMicrosoftAsync(id, request);
                return Ok(new { message = "Microsoft vinculado exitosamente", data = metodo });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al vincular Microsoft", error = ex.Message }); }
        }

        /// <summary>
        /// Desvincula un proveedor de la cuenta del usuario
        /// </summary>
        [HttpDelete("{id}/metodos-auth/{proveedor}")]
        public async Task<IActionResult> DesvincularProveedor(int id, string proveedor)
        {
            try
            {
                await _usuarioService.DesvincularProveedorAsync(id, proveedor);
                return Ok(new { message = $"{proveedor} desvinculado exitosamente" });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al desvincular proveedor", error = ex.Message }); }
        }
    }
}