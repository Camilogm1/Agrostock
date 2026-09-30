using Microsoft.AspNetCore.Mvc;
using AgroStockTaller.Api.DTOs;
using AgroStockTaller.Api.Services;

namespace AgroStockTaller.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CultivosController : ControllerBase
    {
        private readonly ICultivoService _service;
        public CultivosController(ICultivoService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Listar() => Ok(await _service.ListarAsync());

        [HttpPost]
        public async Task<IActionResult> Registrar(CultivoRequest request)
        {
            try
            {
                var resultado = await _service.RegistrarAsync(request);
                return CreatedAtAction(nameof(Listar), resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
