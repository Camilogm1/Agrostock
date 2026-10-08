using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroStock.Api.Services;

namespace AgroStock.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventarioController : ControllerBase
    {
        private readonly IInventarioService _service;
        public InventarioController(IInventarioService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] string? filtro) => Ok(await _service.ListarAsync(filtro));
    }
}
