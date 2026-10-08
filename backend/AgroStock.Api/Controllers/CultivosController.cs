using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroStock.Api.DTOs;
using AgroStock.Api.Exceptions;
using AgroStock.Api.Services;

namespace AgroStock.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CultivosController : ControllerBase
    {
        private readonly ICultivoService _service;
        public CultivosController(ICultivoService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Listar() => Ok(await _service.ListarAsync());

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Registrar(CultivoRequest request)
        {
            try
            {
                var resultado = await _service.RegistrarAsync(request);
                return CreatedAtAction(nameof(Listar), resultado);
            }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Modificar(int id, CultivoRequest request)
        {
            try { return Ok(await _service.ModificarAsync(id, request)); }
            catch (NotFoundException ex) { return NotFound(new { mensaje = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try { await _service.EliminarAsync(id); return NoContent(); }
            catch (CultivoConCosechasException ex) { return Conflict(new { mensaje = ex.Message }); }
            catch (NotFoundException ex) { return NotFound(new { mensaje = ex.Message }); }
        }

        [HttpGet("{id}/cosechas")]
        public async Task<IActionResult> ListarCosechas(int id)
        {
            try { return Ok(await _service.ListarCosechasAsync(id)); }
            catch (NotFoundException ex) { return NotFound(new { mensaje = ex.Message }); }
        }
    }
}
