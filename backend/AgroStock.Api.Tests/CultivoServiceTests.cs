using Microsoft.EntityFrameworkCore;
using AgroStock.Api.Data;
using AgroStock.Api.DTOs;
using AgroStock.Api.Services;
using Xunit;

namespace AgroStock.Api.Tests
{
    public class CultivoServiceTests
    {
        private static AgroStockDbContext CrearDbContext()
        {
            var options = new DbContextOptionsBuilder<AgroStockDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AgroStockDbContext(options);
        }

        [Fact]
        public async Task RegistrarAsync_ConDatosValidos_CreaCultivoConInventarioInicialEnCero()
        {
            await using var db = CrearDbContext();
            var service = new CultivoService(db);
            var request = new CultivoRequest("Tomate Chonto", "Hortaliza", "Lote-A1", new DateTime(2026, 3, 1));

            var resultado = await service.RegistrarAsync(request);

            Assert.True(resultado.IdCultivo > 0);
            var inventario = await db.Inventarios.FirstOrDefaultAsync(i => i.IdCultivo == resultado.IdCultivo);
            Assert.NotNull(inventario);
            Assert.Equal(0, inventario!.CantidadDisponible);
        }

        [Fact]
        public async Task RegistrarAsync_SinNombre_DeberiaFallarLaValidacion()
        {
            await using var db = CrearDbContext();
            var service = new CultivoService(db);
            var request = new CultivoRequest("", "Hortaliza", "Lote-A1", new DateTime(2026, 3, 1));

            await Assert.ThrowsAsync<ArgumentException>(() => service.RegistrarAsync(request));
            Assert.Empty(db.Cultivos);
        }

        [Fact]
        public async Task ListarAsync_DespuesDeRegistrarCultivos_DevuelveTodosLosRegistrados()
        {
            await using var db = CrearDbContext();
            var service = new CultivoService(db);
            await service.RegistrarAsync(new CultivoRequest("Tomate Chonto", "Hortaliza", "A1", new DateTime(2026, 1, 1)));
            await service.RegistrarAsync(new CultivoRequest("Maíz", "Cereal", "B2", new DateTime(2026, 2, 1)));

            var resultado = await service.ListarAsync();

            Assert.Equal(2, resultado.Count);
        }

        [Fact]
        public async Task EliminarAsync_CultivoConCosechas_LanzaExcepcion()
        {
            await using var db = CrearDbContext();
            var service = new CultivoService(db);
            var cultivo = await service.RegistrarAsync(new CultivoRequest("Café", "Permanente", "C1", new DateTime(2026, 1, 1)));

            db.Cosechas.Add(new AgroStock.Api.Models.Cosecha { IdCultivo = cultivo.IdCultivo, Cantidad = 10, Fecha = DateTime.Today });
            await db.SaveChangesAsync();

            await Assert.ThrowsAsync<AgroStock.Api.Exceptions.CultivoConCosechasException>(
                () => service.EliminarAsync(cultivo.IdCultivo)); // RF-04
        }
    }
}
