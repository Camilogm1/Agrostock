using Microsoft.EntityFrameworkCore;
using AgroStock.Api.DTOs;
using AgroStock.Api.Exceptions;
using AgroStock.Api.Services;
using Xunit;

namespace AgroStock.Api.Tests
{
    public class CosechaServiceTests
    {
        [Fact]
        public async Task RegistrarCosechaAsync_ConDatosValidos_IncrementaElInventario()
        {
            await using var db = TestDb.Crear();
            var cultivo = await new CultivoService(db).RegistrarAsync(
                new CultivoRequest("Tomate", "Hortaliza", "A1", new DateTime(2026, 1, 1)));
            var service = new CosechaService(db);

            await service.RegistrarCosechaAsync(new CosechaRequest(cultivo.IdCultivo, 50, new DateTime(2026, 4, 1)));
            await service.RegistrarCosechaAsync(new CosechaRequest(cultivo.IdCultivo, 25.5m, new DateTime(2026, 4, 8)));

            var inventario = await db.Inventarios.SingleAsync(i => i.IdCultivo == cultivo.IdCultivo);
            Assert.Equal(75.5m, inventario.CantidadDisponible); // RF-07
            Assert.Equal(2, await db.Cosechas.CountAsync());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task RegistrarCosechaAsync_CantidadNoPositiva_LanzaArgumentException(decimal cantidad)
        {
            await using var db = TestDb.Crear();
            var cultivo = await new CultivoService(db).RegistrarAsync(
                new CultivoRequest("Tomate", "Hortaliza", "A1", new DateTime(2026, 1, 1)));

            await Assert.ThrowsAsync<ArgumentException>(() => new CosechaService(db)
                .RegistrarCosechaAsync(new CosechaRequest(cultivo.IdCultivo, cantidad, new DateTime(2026, 4, 1)))); // RF-06
        }

        [Fact]
        public async Task RegistrarCosechaAsync_CultivoInexistente_LanzaNotFound()
        {
            await using var db = TestDb.Crear();

            await Assert.ThrowsAsync<NotFoundException>(() => new CosechaService(db)
                .RegistrarCosechaAsync(new CosechaRequest(999, 10, new DateTime(2026, 4, 1)))); // RF-10
            Assert.Empty(db.Cosechas);
        }
    }
}
