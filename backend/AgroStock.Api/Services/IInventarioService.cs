using AgroStock.Api.DTOs;

namespace AgroStock.Api.Services
{
    public interface IInventarioService
    {
        Task<List<InventarioResponse>> ListarAsync(string? filtro);
    }
}
