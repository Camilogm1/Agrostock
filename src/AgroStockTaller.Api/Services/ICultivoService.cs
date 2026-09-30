using AgroStockTaller.Api.DTOs;

namespace AgroStockTaller.Api.Services
{
    public interface ICultivoService
    {
        Task<CultivoResponse> RegistrarAsync(CultivoRequest request); // RF-01
        Task<List<CultivoResponse>> ListarAsync();                    // RF-02
    }
}
