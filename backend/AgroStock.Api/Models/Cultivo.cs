using System.ComponentModel.DataAnnotations;

namespace AgroStock.Api.Models
{
    public class Cultivo
    {
        public int IdCultivo { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Lote { get; set; } = string.Empty;

        public DateTime FechaSiembra { get; set; }

        // Agregación: 0..N cosechas, sin borrado en cascada (RF-04)
        public ICollection<Cosecha> Cosechas { get; set; } = new List<Cosecha>();
    }
}
