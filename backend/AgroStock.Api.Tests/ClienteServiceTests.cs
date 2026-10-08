using AgroStock.Api.DTOs;
using AgroStock.Api.Exceptions;
using AgroStock.Api.Services;
using Xunit;

namespace AgroStock.Api.Tests
{
    public class ClienteServiceTests
    {
        [Fact]
        public async Task RegistrarAsync_IdentificacionRepetida_LanzaConflicto()
        {
            await using var db = TestDb.Crear();
            var service = new ClienteService(db);
            await service.RegistrarAsync(new ClienteRequest("María Restrepo", "1020304050"));

            await Assert.ThrowsAsync<ConflictoException>(() =>
                service.RegistrarAsync(new ClienteRequest("Otra persona", " 1020304050 "))); // RNF-09
            Assert.Single(db.Clientes);
        }

        [Fact]
        public async Task RegistrarAsync_SinIdentificacion_LanzaArgumentException()
        {
            await using var db = TestDb.Crear();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                new ClienteService(db).RegistrarAsync(new ClienteRequest("María", "")));
        }

        [Fact]
        public async Task ListarAsync_ConTexto_FiltraPorNombreOIdentificacion()
        {
            await using var db = TestDb.Crear();
            var service = new ClienteService(db);
            await service.RegistrarAsync(new ClienteRequest("María Restrepo", "1020304050"));
            await service.RegistrarAsync(new ClienteRequest("Pedro Gómez", "900123"));

            Assert.Single(await service.ListarAsync("María"));
            Assert.Single(await service.ListarAsync("900"));
            Assert.Equal(2, (await service.ListarAsync(null)).Count);
        }
    }
}
