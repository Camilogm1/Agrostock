using Microsoft.EntityFrameworkCore;
using AgroStock.Api.Data;

namespace AgroStock.Api.Tests
{
    internal static class TestDb
    {
        // Base en memoria nueva por prueba, para que no compartan datos
        public static AgroStockDbContext Crear()
        {
            var options = new DbContextOptionsBuilder<AgroStockDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AgroStockDbContext(options);
        }
    }
}
