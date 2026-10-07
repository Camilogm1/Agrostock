using Microsoft.EntityFrameworkCore;
using AgroStock.Api.Data;
using AgroStock.Api.DTOs;
using AgroStock.Api.Exceptions;
using AgroStock.Api.Models;

namespace AgroStock.Api.Services
{
    public class CultivoService : ICultivoService
    {
        private readonly AgroStockDbContext _db;
        public CultivoService(AgroStockDbContext db) => _db = db;

        public async Task<CultivoResponse> RegistrarAsync(CultivoRequest request)
        {
            // RNF-06: validar campos obligatorios
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
            await _db.SaveChangesAsync();

            // Inventario inicial en 0, ligado 1-1 al cultivo recién creado
            _db.Inventarios.Add(new Inventario { IdCultivo = cultivo.IdCultivo, CantidadDisponible = 0 });
            await _db.SaveChangesAsync();

            return ToResponse(cultivo);
        }

        public async Task<List<CultivoResponse>> ListarAsync()
        {
            return await _db.Cultivos
                .Select(c => new CultivoResponse(c.IdCultivo, c.Nombre, c.Tipo, c.Lote, c.FechaSiembra))
                .ToListAsync();
        }

        public async Task<CultivoResponse> ModificarAsync(int idCultivo, CultivoRequest request)
        {
            var cultivo = await _db.Cultivos.FindAsync(idCultivo)
                ?? throw new NotFoundException($"Cultivo {idCultivo} no encontrado.");

            cultivo.Nombre = request.Nombre;
            cultivo.Tipo = request.Tipo;
            cultivo.Lote = request.Lote;
            cultivo.FechaSiembra = request.FechaSiembra;

            await _db.SaveChangesAsync();
            return ToResponse(cultivo);
        }

        public async Task EliminarAsync(int idCultivo)
        {
            var cultivo = await _db.Cultivos
                .Include(c => c.Cosechas)
                .FirstOrDefaultAsync(c => c.IdCultivo == idCultivo)
                ?? throw new NotFoundException($"Cultivo {idCultivo} no encontrado.");

            if (cultivo.Cosechas.Any())
                throw new CultivoConCosechasException(idCultivo); // RF-04

            _db.Cultivos.Remove(cultivo);
            await _db.SaveChangesAsync();
        }

        public async Task<List<CosechaResponse>> ListarCosechasAsync(int idCultivo)
        {
            var existe = await _db.Cultivos.AnyAsync(c => c.IdCultivo == idCultivo);
            if (!existe) throw new NotFoundException($"Cultivo {idCultivo} no encontrado.");

            return await _db.Cosechas
                .Where(co => co.IdCultivo == idCultivo)
                .OrderBy(co => co.Fecha)
                .Select(co => new CosechaResponse(co.IdCosecha, co.IdCultivo, co.Cantidad, co.Fecha))
                .ToListAsync();
        }

        private static CultivoResponse ToResponse(Cultivo c) =>
            new(c.IdCultivo, c.Nombre, c.Tipo, c.Lote, c.FechaSiembra);
    }
}
