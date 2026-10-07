namespace AgroStock.Api.DTOs
{
    public record InventarioResponse(
        int IdInventario, int IdCultivo, string NombreProducto,
        decimal CantidadDisponible, DateTime FechaActualizacion);
}
