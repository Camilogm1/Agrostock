using AgroStock.Api.DTOs;

namespace AgroStock.Api.Services
{
    public interface ICultivoService
    {
        Task<CultivoResponse> RegistrarAsync(CultivoRequest request);
        Task<List<CultivoResponse>> ListarAsync(bool incluirArchivados = false);
        Task<CultivoResponse> ModificarAsync(int idCultivo, CultivoRequest request);
        Task EliminarAsync(int idCultivo);
        Task<CultivoResponse> ArchivarAsync(int idCultivo);
        Task<CultivoResponse> RestaurarAsync(int idCultivo);
        Task<List<CosechaResponse>> ListarCosechasAsync(int idCultivo); // RF-11
    }
}
