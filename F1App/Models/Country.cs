using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1App.Models
{
    public class Country
    {
        [Key]
        [Column("country_code")]
        public required string CountryCode { get; set; }

        [Column("name")]
        public required string Name { get; set; }

        [Column("flag_image")]
        public byte[]? FlagImage { get; set; }
    }
}
