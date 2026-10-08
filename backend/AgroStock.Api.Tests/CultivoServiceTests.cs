using Microsoft.EntityFrameworkCore;
using AgroStock.Api.DTOs;
using AgroStock.Api.Exceptions;
using AgroStock.Api.Models;
using AgroStock.Api.Services;
using Xunit;

namespace AgroStock.Api.Tests
{
    public class CultivoServiceTests
    {
        [Fact]
        public async Task RegistrarAsync_ConDatosValidos_CreaCultivoConInventarioInicialEnCero()
        {
            await using var db = TestDb.Crear();
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
            await using var db = TestDb.Crear();
            var service = new CultivoService(db);
            var request = new CultivoRequest("", "Hortaliza", "Lote-A1", new DateTime(2026, 3, 1));

            await Assert.ThrowsAsync<ArgumentException>(() => service.RegistrarAsync(request));
            Assert.Empty(db.Cultivos);
        }

        [Fact]
        public async Task ListarAsync_DespuesDeRegistrarCultivos_DevuelveTodosLosRegistrados()
        {
            await using var db = TestDb.Crear();
            var service = new CultivoService(db);
            await service.RegistrarAsync(new CultivoRequest("Tomate Chonto", "Hortaliza", "A1", new DateTime(2026, 1, 1)));
            await service.RegistrarAsync(new CultivoRequest("Maíz", "Cereal", "B2", new DateTime(2026, 2, 1)));

            var resultado = await service.ListarAsync();

            Assert.Equal(2, resultado.Count);
        }

        [Fact]
        public async Task ModificarAsync_ConDatosValidos_ActualizaElCultivo()
        {
            await using var db = TestDb.Crear();
            var service = new CultivoService(db);
            var cultivo = await service.RegistrarAsync(new CultivoRequest("Tomate", "Hortaliza", "A1", new DateTime(2026, 1, 1)));

            var resultado = await service.ModificarAsync(cultivo.IdCultivo,
                new CultivoRequest("Tomate Chonto", "Hortaliza", "A2", new DateTime(2026, 1, 15)));

            Assert.Equal("Tomate Chonto", resultado.Nombre);
            Assert.Equal("A2", (await db.Cultivos.FindAsync(cultivo.IdCultivo))!.Lote);
        }

        [Fact]
        public async Task ModificarAsync_SinNombre_DeberiaFallarLaValidacion()
        {
            await using var db = TestDb.Crear();
            var service = new CultivoService(db);
            var cultivo = await service.RegistrarAsync(new CultivoRequest("Tomate", "Hortaliza", "A1", new DateTime(2026, 1, 1)));

            await Assert.ThrowsAsync<ArgumentException>(() => service.ModificarAsync(cultivo.IdCultivo,
                new CultivoRequest(" ", "Hortaliza", "A1", new DateTime(2026, 1, 1))));
        }

        [Fact]
        public async Task EliminarAsync_CultivoSinCosechas_EliminaCultivoEInventario()
        {
            await using var db = TestDb.Crear();
            var service = new CultivoService(db);
            var cultivo = await service.RegistrarAsync(new CultivoRequest("Yuca", "Tubérculo", "D1", new DateTime(2026, 1, 1)));

            await service.EliminarAsync(cultivo.IdCultivo);

            Assert.Empty(db.Cultivos);
            Assert.Empty(db.Inventarios);
        }

        [Fact]
        public async Task EliminarAsync_CultivoConCosechas_LanzaExcepcion()
        {
            await using var db = TestDb.Crear();
            var service = new CultivoService(db);
            var cultivo = await service.RegistrarAsync(new CultivoRequest("Café", "Permanente", "C1", new DateTime(2026, 1, 1)));

            db.Cosechas.Add(new Cosecha { IdCultivo = cultivo.IdCultivo, Cantidad = 10, Fecha = DateTime.Today });
            await db.SaveChangesAsync();

            await Assert.ThrowsAsync<CultivoConCosechasException>(() => service.EliminarAsync(cultivo.IdCultivo)); // RF-04
        }

        [Fact]
        public async Task EliminarAsync_CultivoInexistente_LanzaNotFound()
        {
            await using var db = TestDb.Crear();
            var service = new CultivoService(db);

            await Assert.ThrowsAsync<NotFoundException>(() => service.EliminarAsync(999));
        }
    }
}
