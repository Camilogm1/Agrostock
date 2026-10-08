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
            Validar(request);

            var cultivo = new Cultivo
            {
                Nombre = request.Nombre.Trim(),
                Tipo = request.Tipo.Trim(),
                Lote = request.Lote.Trim(),
                FechaSiembra = request.FechaSiembra
            };
            _db.Cultivos.Add(cultivo);
            // RF-01: todo cultivo nace con su inventario en 0 (se guardan juntos en un solo SaveChanges)
            _db.Inventarios.Add(new Inventario { Cultivo = cultivo, CantidadDisponible = 0 });
            await _db.SaveChangesAsync();

            return ToResponse(cultivo);
        }

        public async Task<List<CultivoResponse>> ListarAsync()
        {
            return await _db.Cultivos
                .OrderBy(c => c.Nombre)
                .Select(c => new CultivoResponse(c.IdCultivo, c.Nombre, c.Tipo, c.Lote, c.FechaSiembra))
                .ToListAsync();
        }

        public async Task<CultivoResponse> ModificarAsync(int idCultivo, CultivoRequest request)
        {
            Validar(request);

            var cultivo = await _db.Cultivos.FindAsync(idCultivo)
                ?? throw new NotFoundException($"Cultivo {idCultivo} no encontrado.");

            cultivo.Nombre = request.Nombre.Trim();
            cultivo.Tipo = request.Tipo.Trim();
            cultivo.Lote = request.Lote.Trim();
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

            // Sin cosechas el inventario está en 0 y no tiene ventas, así que se elimina con el cultivo.
            var inventario = await _db.Inventarios.FirstOrDefaultAsync(i => i.IdCultivo == idCultivo);
            if (inventario is not null)
                _db.Inventarios.Remove(inventario);

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

        // RNF-06: campos obligatorios
        private static void Validar(CultivoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del cultivo es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.Tipo))
                throw new ArgumentException("El tipo del cultivo es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.Lote))
                throw new ArgumentException("El lote del cultivo es obligatorio.");
            if (request.FechaSiembra == default)
                throw new ArgumentException("La fecha de siembra es obligatoria.");
        }

        private static CultivoResponse ToResponse(Cultivo c) =>
            new(c.IdCultivo, c.Nombre, c.Tipo, c.Lote, c.FechaSiembra);
    }
}
