namespace AgroStock.Api.DTOs
{
    public record CosechaRequest(int IdCultivo, decimal Cantidad, DateTime Fecha);
    public record CosechaResponse(int IdCosecha, int IdCultivo, decimal Cantidad, DateTime Fecha);
}
