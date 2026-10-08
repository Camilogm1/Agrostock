namespace AgroStock.Api.DTOs
{
    public record VentaRequest(int IdCliente, int IdInventario, decimal Cantidad, DateTime Fecha);
    public record DetalleVentaResponse(int IdInventario, string NombreProducto, decimal Cantidad);
    public record VentaResponse(
        int IdVenta, DateTime Fecha, int IdCliente, string NombreCliente,
        List<DetalleVentaResponse> Detalles);
}
