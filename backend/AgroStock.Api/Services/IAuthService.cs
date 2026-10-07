using AgroStock.Api.DTOs;

namespace AgroStock.Api.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> AutenticarAsync(string nombreUsuario, string contrasena);
    }
}
