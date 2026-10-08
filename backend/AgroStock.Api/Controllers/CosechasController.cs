using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroStock.Api.DTOs;
using AgroStock.Api.Exceptions;
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
        {
            try { return Ok(await _service.RegistrarCosechaAsync(request)); }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
            catch (NotFoundException ex) { return NotFound(new { mensaje = ex.Message }); }
        }
    }
}
