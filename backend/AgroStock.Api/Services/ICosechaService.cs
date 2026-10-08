using AgroStock.Api.DTOs;

namespace AgroStock.Api.Services
{
    public interface ICosechaService
    {
        Task<CosechaResponse> RegistrarCosechaAsync(CosechaRequest request);
    }
}
