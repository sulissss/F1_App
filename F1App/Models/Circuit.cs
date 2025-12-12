using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1App.Models
{
    public class Circuit
    {
        [Key]
        [Column("circuit_id")]
        public int CircuitId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("location")]
        public string? Location { get; set; }

        [Column("country_name")]
        public string? CountryName { get; set; }
    }
}
