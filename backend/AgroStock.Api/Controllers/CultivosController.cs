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
        public async Task<IActionResult> Listar([FromQuery] bool incluirArchivados = false)
            => Ok(await _service.ListarAsync(incluirArchivados));

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

        [HttpPost("{id:int}/archivar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Archivar(int id) => Ok(await _service.ArchivarAsync(id));

        [HttpPost("{id:int}/restaurar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Restaurar(int id) => Ok(await _service.RestaurarAsync(id));

        [HttpGet("{id:int}/cosechas")]
        public async Task<IActionResult> ListarCosechas(int id) => Ok(await _service.ListarCosechasAsync(id));
    }
}
