using AgroStock.Api.DTOs;

namespace AgroStock.Api.Services
{
    public interface IVentaService
    {
        Task<VentaResponse> RegistrarVentaAsync(VentaRequest request);
        Task<List<VentaResponse>> ListarAsync(string? filtroCliente, DateTime? filtroFecha);
    }
}
