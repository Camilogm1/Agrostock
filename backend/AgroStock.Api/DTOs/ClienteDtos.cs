namespace AgroStock.Api.DTOs
{
    public record ClienteRequest(string Nombre, string NumeroIdentificacion);
    public record ClienteResponse(int IdCliente, string Nombre, string NumeroIdentificacion);
}
