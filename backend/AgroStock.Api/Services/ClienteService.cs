using Microsoft.EntityFrameworkCore;
using AgroStock.Api.Data;
using AgroStock.Api.DTOs;
using AgroStock.Api.Models;

namespace AgroStock.Api.Services
{
    public class ClienteService : IClienteService
    {
        private readonly AgroStockDbContext _db;
        public ClienteService(AgroStockDbContext db) => _db = db;

        public async Task<ClienteResponse> RegistrarAsync(ClienteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del cliente es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.NumeroIdentificacion))
                throw new ArgumentException("El número de identificación es obligatorio.");

            var existe = await _db.Clientes.AnyAsync(c => c.NumeroIdentificacion == request.NumeroIdentificacion);
            if (existe)
                throw new InvalidOperationException("Ya existe un cliente con esa identificación."); // RNF-09

            var cliente = new Cliente { Nombre = request.Nombre, NumeroIdentificacion = request.NumeroIdentificacion };
            _db.Clientes.Add(cliente);
            await _db.SaveChangesAsync();

            return new ClienteResponse(cliente.IdCliente, cliente.Nombre, cliente.NumeroIdentificacion);
        }

        public async Task<List<ClienteResponse>> ListarAsync(string? texto)
        {
            var query = _db.Clientes.AsQueryable();
            if (!string.IsNullOrWhiteSpace(texto))
                query = query.Where(c => c.Nombre.Contains(texto) || c.NumeroIdentificacion.Contains(texto));

            return await query
                .Select(c => new ClienteResponse(c.IdCliente, c.Nombre, c.NumeroIdentificacion))
                .ToListAsync();
        }
    }
}
