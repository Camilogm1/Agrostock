using Microsoft.EntityFrameworkCore;
using AgroStock.Api.Data;
using AgroStock.Api.DTOs;
using AgroStock.Api.Exceptions;
using AgroStock.Api.Models;

namespace AgroStock.Api.Services
{
    // Implementa el diagrama de secuencia: transacción, validación de stock
    // (RF-16/RF-18), descuento de inventario (RF-17) y rollback ante fallo.
    public class VentaService : IVentaService
    {
        private readonly AgroStockDbContext _db;
        public VentaService(AgroStockDbContext db) => _db = db;

        public async Task<VentaResponse> RegistrarVentaAsync(VentaRequest request)
        {
            if (request.Cantidad <= 0)
                throw new ArgumentException("La cantidad vendida debe ser mayor a cero.");

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var cliente = await _db.Clientes.FindAsync(request.IdCliente)
                    ?? throw new NotFoundException($"Cliente {request.IdCliente} no encontrado.");

                var inventario = await _db.Inventarios
                    .Include(i => i.Cultivo)
                    .FirstOrDefaultAsync(i => i.IdInventario == request.IdInventario)
                    ?? throw new NotFoundException($"Inventario {request.IdInventario} no encontrado.");

                if (!inventario.HayStockSuficiente(request.Cantidad)) // RF-16/RF-18
                {
                    await transaction.RollbackAsync();
                    throw new StockInsuficienteException(inventario.IdInventario, request.Cantidad, inventario.CantidadDisponible);
                }

                var venta = new Venta { IdCliente = cliente.IdCliente, Fecha = request.Fecha };
                _db.Ventas.Add(venta);
                await _db.SaveChangesAsync();

                var detalle = new DetalleVenta
                {
                    IdVenta = venta.IdVenta,
                    IdInventario = inventario.IdInventario,
                    Cantidad = request.Cantidad
                };
                _db.DetallesVenta.Add(detalle);

                inventario.DescontarStock(request.Cantidad); // RF-17
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return new VentaResponse(
                    venta.IdVenta, venta.Fecha, cliente.IdCliente, cliente.Nombre,
                    new List<DetalleVentaResponse> { new(inventario.IdInventario, ObtenerNombreProducto(inventario), request.Cantidad) });
            }
            catch
            {
                if (transaction.GetDbTransaction().Connection != null)
                    await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<VentaResponse>> ListarAsync(string? filtroCliente, DateTime? filtroFecha)
        {
            var query = _db.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles).ThenInclude(d => d.Inventario).ThenInclude(i => i!.Cultivo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtroCliente))
                query = query.Where(v => v.Cliente!.Nombre.Contains(filtroCliente));
            if (filtroFecha.HasValue)
                query = query.Where(v => v.Fecha.Date == filtroFecha.Value.Date);

            var ventas = await query.ToListAsync();

            return ventas.Select(v => new VentaResponse(
                v.IdVenta, v.Fecha, v.IdCliente, v.Cliente!.Nombre,
                v.Detalles.Select(d => new DetalleVentaResponse(
                    d.IdInventario, ObtenerNombreProducto(d.Inventario!), d.Cantidad)).ToList()
            )).ToList();
        }

        private static string ObtenerNombreProducto(Inventario inventario) =>
            inventario.Cultivo?.Nombre ?? $"Cultivo #{inventario.IdCultivo}";
    }
}
