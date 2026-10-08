using AgroStock.Api.DTOs;

namespace AgroStock.Api.Services
{
    public interface ICultivoService
    {
        Task<CultivoResponse> RegistrarAsync(CultivoRequest request);
        Task<List<CultivoResponse>> ListarAsync();
        Task<CultivoResponse> ModificarAsync(int idCultivo, CultivoRequest request);
        Task EliminarAsync(int idCultivo);
        Task<List<CosechaResponse>> ListarCosechasAsync(int idCultivo); // RF-11
    }
}
