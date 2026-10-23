using Microsoft.EntityFrameworkCore;
using AgroStock.Api.Data;
using AgroStock.Api.DTOs;
using AgroStock.Api.Exceptions;
using AgroStock.Api.Services;
using Xunit;

namespace AgroStock.Api.Tests
{
    public class ArchivarCultivoTests
    {
        // Cultivo con 50 cosechados y vendidos en su totalidad: queda con historial y stock en 0
        private static async Task<(int idCultivo, int idInventario)> CultivoVendidoAsync(AgroStockDbContext db)
        {
            var cultivo = await new CultivoService(db).RegistrarAsync(
                new CultivoRequest("Tomate", "Hortaliza", "A1", new DateTime(2026, 1, 1)));
            await new CosechaService(db).RegistrarCosechaAsync(new CosechaRequest(cultivo.IdCultivo, 50, new DateTime(2026, 4, 1)));
            var cliente = await new ClienteService(db).RegistrarAsync(new ClienteRequest("María Restrepo", "1020304050"));
            var inventario = await db.Inventarios.SingleAsync(i => i.IdCultivo == cultivo.IdCultivo);
            await new VentaService(db).RegistrarVentaAsync(
                new VentaRequest(cliente.IdCliente, inventario.IdInventario, 50, new DateTime(2026, 4, 2)));
            return (cultivo.IdCultivo, inventario.IdInventario);
        }

        [Fact]
        public async Task CultivoVendido_SeArchivaYConservaElHistorial()
        {
            await using var db = TestDb.Crear();
            var (idCultivo, _) = await CultivoVendidoAsync(db);
            var service = new CultivoService(db);

            var archivado = await service.ArchivarAsync(idCultivo);

            Assert.False(archivado.Activo);
            Assert.Empty(await service.ListarAsync());
            Assert.Single(await service.ListarAsync(incluirArchivados: true));
            Assert.Single(await service.ListarCosechasAsync(idCultivo));                 // RF-11
            var venta = Assert.Single(await new VentaService(db).ListarAsync(null, null)); // RF-19
            Assert.Equal("Tomate", Assert.Single(venta.Detalles).NombreProducto);
        }

        [Fact]
        public async Task ArchivarAsync_ConStockDisponible_LanzaConflicto()
        {
            await using var db = TestDb.Crear();
            var cultivo = await new CultivoService(db).RegistrarAsync(
                new CultivoRequest("Papa", "Tubérculo", "B1", new DateTime(2026, 1, 1)));
            await new CosechaService(db).RegistrarCosechaAsync(new CosechaRequest(cultivo.IdCultivo, 30, new DateTime(2026, 4, 1)));

            await Assert.ThrowsAsync<CultivoConStockException>(() => new CultivoService(db).ArchivarAsync(cultivo.IdCultivo));
            Assert.True((await db.Cultivos.FindAsync(cultivo.IdCultivo))!.Activo);
        }

        [Fact]
        public async Task CultivoVendido_SeEliminaConSusCosechas_YLasVentasConservanElProducto()
        {
            await using var db = TestDb.Crear();
            var (idCultivo, idInventario) = await CultivoVendidoAsync(db);

            await new CultivoService(db).EliminarAsync(idCultivo);

            Assert.Empty(db.Cultivos);
            Assert.Empty(db.Cosechas);
            Assert.Empty(db.Inventarios);
            var venta = Assert.Single(await new VentaService(db).ListarAsync(null, null)); // RF-19
            var detalle = Assert.Single(venta.Detalles);
            Assert.Equal("Tomate", detalle.NombreProducto);
            Assert.Null(detalle.IdInventario);
        }

        [Fact]
        public async Task CultivoArchivado_NoAdmiteCosechasNiApareceEnInventario()
        {
            await using var db = TestDb.Crear();
            var (idCultivo, _) = await CultivoVendidoAsync(db);
            await new CultivoService(db).ArchivarAsync(idCultivo);

            await Assert.ThrowsAsync<ConflictoException>(() => new CosechaService(db)
                .RegistrarCosechaAsync(new CosechaRequest(idCultivo, 10, new DateTime(2026, 5, 1))));
            Assert.Empty(await new InventarioService(db).ListarAsync(null));
        }

        [Fact]
        public async Task RestaurarAsync_VuelveAMostrarElCultivoYPermiteCosechar()
        {
            await using var db = TestDb.Crear();
            var (idCultivo, _) = await CultivoVendidoAsync(db);
            var service = new CultivoService(db);
            await service.ArchivarAsync(idCultivo);

            var restaurado = await service.RestaurarAsync(idCultivo);
            await new CosechaService(db).RegistrarCosechaAsync(new CosechaRequest(idCultivo, 10, new DateTime(2026, 5, 1)));

            Assert.True(restaurado.Activo);
            Assert.Single(await service.ListarAsync());
            Assert.Single(await new InventarioService(db).ListarAsync(null));
        }
    }
}
