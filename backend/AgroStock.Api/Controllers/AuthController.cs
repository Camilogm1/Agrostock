using Microsoft.AspNetCore.Mvc;
using AgroStock.Api.DTOs;
using AgroStock.Api.Services;

namespace AgroStock.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.AutenticarAsync(request.NombreUsuario, request.Contrasena);
            if (result is null)
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
            return Ok(result);
        }
    }
}
