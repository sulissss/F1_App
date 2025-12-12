using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1App.Models
{
    public class Team
    {
        [Key]
        [Column("team_id")]
        public int TeamId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("short_name")]
        public string? ShortName { get; set; }

        [Column("colour_hex")]
        public string? ColourHex { get; set; }
    }
}
