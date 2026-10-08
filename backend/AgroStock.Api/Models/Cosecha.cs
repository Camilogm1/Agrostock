using System.ComponentModel.DataAnnotations;

namespace AgroStock.Api.Models
{
    public class Cosecha
    {
        public int IdCosecha { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public decimal Cantidad { get; set; }

        public DateTime Fecha { get; set; }

        public int IdCultivo { get; set; }
        public Cultivo? Cultivo { get; set; }
    }
}
