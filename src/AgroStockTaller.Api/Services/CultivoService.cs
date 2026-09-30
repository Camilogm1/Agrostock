using Microsoft.EntityFrameworkCore;
using AgroStockTaller.Api.Data;
using AgroStockTaller.Api.DTOs;
using AgroStockTaller.Api.Models;

namespace AgroStockTaller.Api.Services
{
    public class CultivoService : ICultivoService
    {
        private readonly AppDbContext _db;
        public CultivoService(AppDbContext db) => _db = db;

        public async Task<CultivoResponse> RegistrarAsync(CultivoRequest request)
        {
            // RNF-06: validar que los campos obligatorios no estén vacíos
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del cultivo es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.Tipo))
                throw new ArgumentException("El tipo del cultivo es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.Lote))
                throw new ArgumentException("El lote del cultivo es obligatorio.");

            var cultivo = new Cultivo
            {
                Nombre = request.Nombre,
                Tipo = request.Tipo,
                Lote = request.Lote,
                FechaSiembra = request.FechaSiembra
            };
            _db.Cultivos.Add(cultivo);
            await _db.SaveChangesAsync(); // genera cultivo.IdCultivo

            // RF-07 (simplificado): inventario inicial en 0 al crear el cultivo
            _db.Inventarios.Add(new Inventario { IdCultivo = cultivo.IdCultivo, CantidadDisponible = 0 });
            await _db.SaveChangesAsync();

            return new CultivoResponse(cultivo.IdCultivo, cultivo.Nombre, cultivo.Tipo, cultivo.Lote, cultivo.FechaSiembra);
        }

        public async Task<List<CultivoResponse>> ListarAsync()
        {
            return await _db.Cultivos
                .Select(c => new CultivoResponse(c.IdCultivo, c.Nombre, c.Tipo, c.Lote, c.FechaSiembra))
                .ToListAsync();
        }
    }
}
