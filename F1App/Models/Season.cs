using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1App.Models
{
    public class Season
    {
        [Key]
        [Column("season_id")]
        public int SeasonId { get; set; }

        [Column("year")]
        public int Year { get; set; }

        [Column("status")]
        public string Status { get; set; } = "active";
    }
}
