using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AgroStock.Api.Data;
using AgroStock.Api.DTOs;

namespace AgroStock.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AgroStockDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(AgroStockDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<LoginResponse?> AutenticarAsync(string nombreUsuario, string contrasena)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
            if (usuario is null) return null;

            var esValido = BCrypt.Net.BCrypt.Verify(contrasena, usuario.ContrasenaHash);
            if (!esValido) return null;

            var token = GenerarToken(usuario.NombreUsuario, usuario.Rol.ToString());
            return new LoginResponse(token, usuario.NombreUsuario, usuario.Rol.ToString());
        }

        private string GenerarToken(string nombreUsuario, string rol)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, nombreUsuario),
                new Claim(ClaimTypes.Role, rol)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
