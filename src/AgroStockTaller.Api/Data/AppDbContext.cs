using Microsoft.EntityFrameworkCore;
using AgroStockTaller.Api.Models;

namespace AgroStockTaller.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Cultivo> Cultivos => Set<Cultivo>();
        public DbSet<Inventario> Inventarios => Set<Inventario>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cultivo>().HasKey(c => c.IdCultivo);
            modelBuilder.Entity<Inventario>().HasKey(i => i.IdInventario);
        }
    }
}