namespace AgroStockTaller.Api.DTOs
{
    public record CultivoRequest(string Nombre, string Tipo, string Lote, DateTime FechaSiembra);
    public record CultivoResponse(int IdCultivo, string Nombre, string Tipo, string Lote, DateTime FechaSiembra);
}
