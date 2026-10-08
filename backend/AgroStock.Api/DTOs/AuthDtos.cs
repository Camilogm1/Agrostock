namespace AgroStock.Api.DTOs
{
    public record LoginRequest(string NombreUsuario, string Contrasena);
    public record LoginResponse(string Token, string NombreUsuario, string Rol);
}
