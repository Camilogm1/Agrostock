using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroStock.Api.DTOs;
using AgroStock.Api.Services;

namespace AgroStock.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class CosechasController : ControllerBase
    {
        private readonly ICosechaService _service;
        public CosechasController(ICosechaService service) => _service = service;

        [HttpPost]
        public async Task<IActionResult> Registrar(CosechaRequest request)
            => StatusCode(StatusCodes.Status201Created, await _service.RegistrarCosechaAsync(request));
    }
}
