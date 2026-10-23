using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using AgroStock.Api.Data;
using AgroStock.Api.DTOs;
using AgroStock.Api.Exceptions;
using AgroStock.Api.Models;

namespace AgroStock.Api.Services
{
    public class ClienteService : IClienteService
    {
        // Letras (con tildes y ñ), espacios y los signos de nombres como "O'Neill", "Ma. José" o "Ana-María"
        private static readonly Regex NombreValido = new(@"^\p{L}[\p{L} .'-]*$");
        private static readonly Regex IdentificacionValida = new(@"^[0-9]{6,15}$");

        private readonly AgroStockDbContext _db;
        public ClienteService(AgroStockDbContext db) => _db = db;

        public async Task<ClienteResponse> RegistrarAsync(ClienteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del cliente es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.NumeroIdentificacion))
                throw new ArgumentException("El número de identificación es obligatorio.");

            var nombre = Regex.Replace(request.Nombre.Trim(), @"\s+", " ");
            if (nombre.Length < 3 || nombre.Length > 150)
                throw new ArgumentException("El nombre debe tener entre 3 y 150 caracteres.");
            if (!NombreValido.IsMatch(nombre))
                throw new ArgumentException("El nombre solo puede contener letras y espacios (sin números ni símbolos).");

            var identificacion = request.NumeroIdentificacion.Trim();
            if (!IdentificacionValida.IsMatch(identificacion))
                throw new ArgumentException("El número de identificación debe tener solo números, entre 6 y 15 dígitos.");

            var existe = await _db.Clientes.AnyAsync(c => c.NumeroIdentificacion == identificacion);
            if (existe)
                throw new ConflictoException("Ya existe un cliente con esa identificación."); // RNF-09

            var cliente = new Cliente { Nombre = nombre, NumeroIdentificacion = identificacion };
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
                .OrderBy(c => c.Nombre)
                .Select(c => new ClienteResponse(c.IdCliente, c.Nombre, c.NumeroIdentificacion))
                .ToListAsync();
        }
    }
}
