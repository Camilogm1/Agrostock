using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroStock.Api.DTOs;
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
            => StatusCode(StatusCodes.Status201Created, await _service.RegistrarAsync(request));

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Modificar(int id, CultivoRequest request)
            => Ok(await _service.ModificarAsync(id, request));

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _service.EliminarAsync(id);
            return NoContent();
        }

        [HttpGet("{id:int}/cosechas")]
        public async Task<IActionResult> ListarCosechas(int id) => Ok(await _service.ListarCosechasAsync(id));
    }
}
