using Microsoft.EntityFrameworkCore;
using AgroStock.Api.Data;
using AgroStock.Api.DTOs;
using AgroStock.Api.Exceptions;
using AgroStock.Api.Models;

namespace AgroStock.Api.Services
{
    // Diagrama de secuencia de la venta: validar stock (RF-16/RF-18), registrar la venta
    // y descontar el inventario (RF-17) de forma atómica.
    public class VentaService : IVentaService
    {
        private readonly AgroStockDbContext _db;
        public VentaService(AgroStockDbContext db) => _db = db;

        public async Task<VentaResponse> RegistrarVentaAsync(VentaRequest request)
        {
            if (request.Cantidad <= 0)
                throw new ArgumentException("La cantidad vendida debe ser mayor a cero.");
            if (request.Fecha == default)
                throw new ArgumentException("La fecha de la venta es obligatoria.");

            var cliente = await _db.Clientes.FindAsync(request.IdCliente)
                ?? throw new NotFoundException($"Cliente {request.IdCliente} no encontrado.");

            var inventario = await _db.Inventarios
                .Include(i => i.Cultivo)
                .FirstOrDefaultAsync(i => i.IdInventario == request.IdInventario)
                ?? throw new NotFoundException($"Inventario {request.IdInventario} no encontrado.");

            inventario.DescontarStock(request.Cantidad); // lanza StockInsuficienteException si no alcanza

            var venta = new Venta { IdCliente = cliente.IdCliente, Fecha = request.Fecha };
            venta.Detalles.Add(new DetalleVenta { Inventario = inventario, Cantidad = request.Cantidad });
            _db.Ventas.Add(venta);

            // Un solo SaveChanges = una transacción (venta + detalle + stock). Si otra venta cambió
            // el stock mientras tanto, el token de concurrencia hace fallar el guardado (409).
            await _db.SaveChangesAsync();

            return new VentaResponse(
                venta.IdVenta, venta.Fecha, cliente.IdCliente, cliente.Nombre,
                new List<DetalleVentaResponse> { new(inventario.IdInventario, ObtenerNombreProducto(inventario), request.Cantidad) });
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
            {
                var desde = filtroFecha.Value.Date;
                var hasta = desde.AddDays(1);
                query = query.Where(v => v.Fecha >= desde && v.Fecha < hasta);
            }

            var ventas = await query.OrderByDescending(v => v.Fecha).ThenByDescending(v => v.IdVenta).ToListAsync();

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
