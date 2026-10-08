using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroStock.Api.DTOs;
using AgroStock.Api.Services;

namespace AgroStock.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _service;
        public ClientesController(IClienteService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] string? texto) => Ok(await _service.ListarAsync(texto));

        [HttpPost]
        public async Task<IActionResult> Registrar(ClienteRequest request)
            => StatusCode(StatusCodes.Status201Created, await _service.RegistrarAsync(request));
    }
}
