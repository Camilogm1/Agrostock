using Microsoft.EntityFrameworkCore;
using AgroStock.Api.Data;
using AgroStock.Api.DTOs;
using AgroStock.Api.Exceptions;
using AgroStock.Api.Models;

namespace AgroStock.Api.Services
{
    public class CosechaService : ICosechaService
    {
        private readonly AgroStockDbContext _db;
        public CosechaService(AgroStockDbContext db) => _db = db;

        public async Task<CosechaResponse> RegistrarCosechaAsync(CosechaRequest request)
        {
            if (request.Cantidad <= 0)
                throw new ArgumentException("La cantidad cosechada debe ser mayor a cero."); // RF-06

            var cultivo = await _db.Cultivos.FindAsync(request.IdCultivo)
                ?? throw new NotFoundException($"Cultivo {request.IdCultivo} no encontrado."); // RF-10

            await using var transaction = await _db.Database.BeginTransactionAsync();

            var cosecha = new Cosecha
            {
                IdCultivo = cultivo.IdCultivo,
                Cantidad = request.Cantidad,
                Fecha = request.Fecha
            };
            _db.Cosechas.Add(cosecha);

            var inventario = await _db.Inventarios.FirstOrDefaultAsync(i => i.IdCultivo == cultivo.IdCultivo);
            if (inventario is null)
            {
                inventario = new Inventario { IdCultivo = cultivo.IdCultivo, CantidadDisponible = 0 };
                _db.Inventarios.Add(inventario);
            }
            inventario.IncrementarStock(request.Cantidad); // RF-07

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new CosechaResponse(cosecha.IdCosecha, cosecha.IdCultivo, cosecha.Cantidad, cosecha.Fecha);
        }
    }
}
