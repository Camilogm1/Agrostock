using Microsoft.EntityFrameworkCore;
using AgroStock.Api.Models;

namespace AgroStock.Api.Data
{
    // Aplica las migraciones pendientes y, si no hay usuarios, crea los de la sección
    // "UsuariosIniciales" de la configuración (sin ellos nadie podría iniciar sesión).
    public static class DatosIniciales
    {
        public static async Task InicializarAsync(IServiceProvider services, IConfiguration config)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AgroStockDbContext>();

            await db.Database.MigrateAsync();

            if (await db.Usuarios.AnyAsync()) return;

            var usuarios = config.GetSection("UsuariosIniciales").Get<List<UsuarioInicial>>() ?? new();
            foreach (var u in usuarios)
            {
                db.Usuarios.Add(new Usuario
                {
                    NombreUsuario = u.NombreUsuario,
                    ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(u.Contrasena),
                    Rol = u.Rol
                });
            }
            await db.SaveChangesAsync();
        }

        private record UsuarioInicial(string NombreUsuario, string Contrasena, Rol Rol);
    }
}
