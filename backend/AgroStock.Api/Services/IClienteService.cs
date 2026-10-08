using AgroStock.Api.DTOs;

namespace AgroStock.Api.Services
{
    public interface IClienteService
    {
        Task<ClienteResponse> RegistrarAsync(ClienteRequest request);
        Task<List<ClienteResponse>> ListarAsync(string? texto);
    }
}
