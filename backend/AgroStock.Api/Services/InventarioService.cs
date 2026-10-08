using Microsoft.EntityFrameworkCore;
using AgroStock.Api.Data;
using AgroStock.Api.DTOs;

namespace AgroStock.Api.Services
{
    public class InventarioService : IInventarioService
    {
        private readonly AgroStockDbContext _db;
        public InventarioService(AgroStockDbContext db) => _db = db;

        public async Task<List<InventarioResponse>> ListarAsync(string? filtro)
        {
            var query = _db.Inventarios.Include(i => i.Cultivo).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro))
                query = query.Where(i => i.Cultivo!.Nombre.Contains(filtro) || i.Cultivo!.Tipo.Contains(filtro));

            return await query.Select(i => new InventarioResponse(
                i.IdInventario, i.IdCultivo, i.Cultivo!.Nombre,
                i.CantidadDisponible, i.FechaActualizacion)).ToListAsync();
        }
    }
}
