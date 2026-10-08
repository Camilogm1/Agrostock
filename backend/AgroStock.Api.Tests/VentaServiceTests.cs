using Microsoft.EntityFrameworkCore;
using AgroStock.Api.DTOs;
using AgroStock.Api.Exceptions;
using AgroStock.Api.Services;
using Xunit;

namespace AgroStock.Api.Tests
{
    public class VentaServiceTests
    {
        // Deja un cliente y un cultivo con 50 unidades en inventario
        private static async Task<(int idCliente, int idInventario)> PrepararDatosAsync(Data.AgroStockDbContext db)
        {
            var cultivo = await new CultivoService(db).RegistrarAsync(
                new CultivoRequest("Tomate", "Hortaliza", "A1", new DateTime(2026, 1, 1)));
            await new CosechaService(db).RegistrarCosechaAsync(new CosechaRequest(cultivo.IdCultivo, 50, new DateTime(2026, 4, 1)));
            var cliente = await new ClienteService(db).RegistrarAsync(new ClienteRequest("María Restrepo", "1020304050"));
            var inventario = await db.Inventarios.SingleAsync(i => i.IdCultivo == cultivo.IdCultivo);
            return (cliente.IdCliente, inventario.IdInventario);
        }

        [Fact]
        public async Task RegistrarVentaAsync_ConStockSuficiente_RegistraVentaYDescuentaInventario()
        {
            await using var db = TestDb.Crear();
            var (idCliente, idInventario) = await PrepararDatosAsync(db);

            var venta = await new VentaService(db).RegistrarVentaAsync(
                new VentaRequest(idCliente, idInventario, 20, new DateTime(2026, 4, 2)));

            Assert.True(venta.IdVenta > 0);
            Assert.Equal("Tomate", Assert.Single(venta.Detalles).NombreProducto);
            Assert.Equal(30, (await db.Inventarios.FindAsync(idInventario))!.CantidadDisponible); // RF-17
        }

        [Fact]
        public async Task RegistrarVentaAsync_StockInsuficiente_NoRegistraNadaNiDescuenta()
        {
            await using var db = TestDb.Crear();
            var (idCliente, idInventario) = await PrepararDatosAsync(db);

            await Assert.ThrowsAsync<StockInsuficienteException>(() => new VentaService(db).RegistrarVentaAsync(
                new VentaRequest(idCliente, idInventario, 51, new DateTime(2026, 4, 2)))); // RF-16/RF-18

            Assert.Empty(db.Ventas);
            Assert.Equal(50, (await db.Inventarios.FindAsync(idInventario))!.CantidadDisponible);
        }

        [Fact]
        public async Task RegistrarVentaAsync_ClienteInexistente_LanzaNotFound()
        {
            await using var db = TestDb.Crear();
            var (_, idInventario) = await PrepararDatosAsync(db);

            await Assert.ThrowsAsync<NotFoundException>(() => new VentaService(db).RegistrarVentaAsync(
                new VentaRequest(999, idInventario, 5, new DateTime(2026, 4, 2))));
        }

        [Fact]
        public async Task RegistrarVentaAsync_CantidadCero_LanzaArgumentException()
        {
            await using var db = TestDb.Crear();
            var (idCliente, idInventario) = await PrepararDatosAsync(db);

            await Assert.ThrowsAsync<ArgumentException>(() => new VentaService(db).RegistrarVentaAsync(
                new VentaRequest(idCliente, idInventario, 0, new DateTime(2026, 4, 2))));
        }

        [Fact]
        public async Task ListarAsync_FiltraPorClienteYFecha()
        {
            await using var db = TestDb.Crear();
            var (idCliente, idInventario) = await PrepararDatosAsync(db);
            var service = new VentaService(db);
            await service.RegistrarVentaAsync(new VentaRequest(idCliente, idInventario, 5, new DateTime(2026, 4, 2)));
            await service.RegistrarVentaAsync(new VentaRequest(idCliente, idInventario, 5, new DateTime(2026, 4, 3)));

            Assert.Equal(2, (await service.ListarAsync("María", null)).Count);
            Assert.Empty(await service.ListarAsync("Pedro", null));
            Assert.Single(await service.ListarAsync(null, new DateTime(2026, 4, 3)));
        }
    }
}
