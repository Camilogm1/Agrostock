using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroStock.Api.DTOs;
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
            => StatusCode(StatusCodes.Status201Created, await _service.RegistrarVentaAsync(request));
    }
}
