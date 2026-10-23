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

        [Theory]
        [InlineData("Ana")]
        [InlineData("José Ñañez")]
        [InlineData("María-José O'Neill")]
        public async Task RegistrarAsync_NombresValidos_SeRegistran(string nombre)
        {
            await using var db = TestDb.Crear();

            var cliente = await new ClienteService(db).RegistrarAsync(new ClienteRequest(nombre, "1020304050"));

            Assert.Equal(nombre, cliente.Nombre);
        }

        [Theory]
        [InlineData("Jo")]          // muy corto
        [InlineData("Juan123")]     // números
        [InlineData("Juan@Pérez")]  // símbolos
        [InlineData("-Juan")]       // no empieza por letra
        public async Task RegistrarAsync_NombreInvalido_LanzaArgumentException(string nombre)
        {
            await using var db = TestDb.Crear();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                new ClienteService(db).RegistrarAsync(new ClienteRequest(nombre, "1020304050")));
            Assert.Empty(db.Clientes);
        }

        [Fact]
        public async Task RegistrarAsync_NombreConEspaciosDeMas_SeNormaliza()
        {
            await using var db = TestDb.Crear();

            var cliente = await new ClienteService(db).RegistrarAsync(new ClienteRequest("  María   Restrepo ", "1020304050"));

            Assert.Equal("María Restrepo", cliente.Nombre);
        }

        [Theory]
        [InlineData("10203A4050")]  // letras
        [InlineData("1.020.304")]   // puntos
        [InlineData("900123-4")]    // guion
        [InlineData("12345")]       // muy corta
        [InlineData("1234567890123456")] // muy larga
        public async Task RegistrarAsync_IdentificacionNoNumerica_LanzaArgumentException(string identificacion)
        {
            await using var db = TestDb.Crear();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                new ClienteService(db).RegistrarAsync(new ClienteRequest("María Restrepo", identificacion)));
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
