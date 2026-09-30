namespace AgroStockTaller.Api.Models
{
    public class Cultivo
    {
        public int IdCultivo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Lote { get; set; } = string.Empty;
        public DateTime FechaSiembra { get; set; }
    }
}
