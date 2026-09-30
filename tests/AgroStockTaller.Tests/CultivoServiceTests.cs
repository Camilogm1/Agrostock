using Microsoft.EntityFrameworkCore;
using AgroStockTaller.Api.Data;
using AgroStockTaller.Api.DTOs;
using AgroStockTaller.Api.Services;
using Xunit;

namespace AgroStockTaller.Tests
{
    public class CultivoServiceTests
    {
        private static AppDbContext CrearDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        // ============================================================
        // PRUEBA 1 — derivada del criterio de aceptación Dado/Cuando/Entonces
        // de la Entrega 1 para RF-01 (Registro de cultivos).
        // ============================================================
        [Fact]
        public async Task RegistrarAsync_ConDatosValidos_CreaCultivoConInventarioInicialEnCero()
        {
            // Dado: un administrador y datos válidos de cultivo
            await using var db = CrearDbContext();
            var service = new CultivoService(db);
            var request = new CultivoRequest("Tomate Chonto", "Hortaliza", "Lote-A1", new DateTime(2026, 3, 1));

            // Cuando: se registra el cultivo
            var resultado = await service.RegistrarAsync(request);

            // Entonces: el cultivo queda creado...
            Assert.True(resultado.IdCultivo > 0);
            Assert.Equal("Tomate Chonto", resultado.Nombre);

            // ...y su inventario inicial se crea automáticamente en 0
            var inventario = await db.Inventarios.FirstOrDefaultAsync(i => i.IdCultivo == resultado.IdCultivo);
            Assert.NotNull(inventario);
            Assert.Equal(0, inventario!.CantidadDisponible);
        }

        // ============================================================
        // PRUEBA 2 — RNF-06: validar que los campos obligatorios no se
        // guarden vacíos.
        // ============================================================
        [Fact]
        public async Task RegistrarAsync_SinNombre_DeberiaFallarLaValidacion()
        {
            await using var db = CrearDbContext();
            var service = new CultivoService(db);
            var request = new CultivoRequest("", "Hortaliza", "Lote-A1", new DateTime(2026, 3, 1));

            var excepcion = await Assert.ThrowsAsync<ArgumentException>(
                () => service.RegistrarAsync(request));

            Assert.Contains("nombre", excepcion.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(db.Cultivos);
        }

        // ============================================================
        // PRUEBA 3 — RF-02: consultar el listado de cultivos registrados.
        // ============================================================
        [Fact]
        public async Task ListarAsync_DespuesDeRegistrarCultivos_DevuelveTodosLosRegistrados()
        {
            await using var db = CrearDbContext();
            var service = new CultivoService(db);
            await service.RegistrarAsync(new CultivoRequest("Tomate Chonto", "Hortaliza", "A1", new DateTime(2026, 1, 1)));
            await service.RegistrarAsync(new CultivoRequest("Maíz", "Cereal", "B2", new DateTime(2026, 2, 1)));

            var resultado = await service.ListarAsync();

            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, c => c.Nombre == "Tomate Chonto");
            Assert.Contains(resultado, c => c.Nombre == "Maíz");
        }
    }
}
