using Microsoft.EntityFrameworkCore;
using AgroStock.Api.Models;

namespace AgroStock.Api.Data
{
    public class AgroStockDbContext : DbContext
    {
        public AgroStockDbContext(DbContextOptions<AgroStockDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Cultivo> Cultivos => Set<Cultivo>();
        public DbSet<Cosecha> Cosechas => Set<Cosecha>();
        public DbSet<Inventario> Inventarios => Set<Inventario>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Todas las claves se declaran explícitamente: el prefijo "Id" (IdCultivo,
            // IdCosecha, etc.) no coincide con la convención automática de EF Core
            // ("Id" o "ClaseId"), así que hay que indicarlas a mano en cada entidad.

            modelBuilder.Entity<Usuario>(e =>
            {
                e.HasKey(u => u.IdUsuario);
                e.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(50);
                e.HasIndex(u => u.NombreUsuario).IsUnique();
                e.Property(u => u.Rol).HasConversion<string>();
            });

            modelBuilder.Entity<Cultivo>(e =>
            {
                e.HasKey(c => c.IdCultivo);
                e.HasMany(c => c.Cosechas)
                 .WithOne(co => co.Cultivo)
                 .HasForeignKey(co => co.IdCultivo)
                 .OnDelete(DeleteBehavior.Restrict); // RF-04: sin borrado en cascada
            });

            modelBuilder.Entity<Cosecha>(e =>
            {
                e.HasKey(c => c.IdCosecha);
                e.Property(c => c.Cantidad).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Inventario>(e =>
            {
                e.HasKey(i => i.IdInventario);
                e.Property(i => i.CantidadDisponible).HasColumnType("decimal(18,2)");
                e.HasIndex(i => i.IdCultivo).IsUnique(); // relación 1-1 Cultivo-Inventario
                e.HasOne(i => i.Cultivo)
                 .WithMany()
                 .HasForeignKey(i => i.IdCultivo)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Cliente>(e =>
            {
                e.HasKey(c => c.IdCliente);
                e.HasIndex(c => c.NumeroIdentificacion).IsUnique(); // RNF-09
            });

            modelBuilder.Entity<Venta>(e =>
            {
                e.HasKey(v => v.IdVenta);
                e.HasOne(v => v.Cliente)
                 .WithMany()
                 .HasForeignKey(v => v.IdCliente)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(v => v.Detalles)
                 .WithOne(d => d.Venta)
                 .HasForeignKey(d => d.IdVenta)
                 .OnDelete(DeleteBehavior.Cascade); // composición
            });

            modelBuilder.Entity<DetalleVenta>(e =>
            {
                e.HasKey(d => d.IdDetalleVenta);
                e.Property(d => d.Cantidad).HasColumnType("decimal(18,2)");
                e.HasOne(d => d.Inventario)
                 .WithMany()
                 .HasForeignKey(d => d.IdInventario)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
