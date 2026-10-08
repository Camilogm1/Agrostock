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
    public class VentasController : ControllerBase
    {
        private readonly IVentaService _service;
        public VentasController(IVentaService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] string? cliente, [FromQuery] DateTime? fecha)
            => Ok(await _service.ListarAsync(cliente, fecha));

        [HttpPost]
        public async Task<IActionResult> Registrar(VentaRequest request)
        {
            try
            {
                var resultado = await _service.RegistrarVentaAsync(request);
                return StatusCode(201, resultado);
            }
            catch (StockInsuficienteException ex) { return UnprocessableEntity(new { mensaje = ex.Message }); }
            catch (NotFoundException ex) { return NotFound(new { mensaje = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        }
    }
}
