using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1App.Models
{
    public class Event
    {
        [Key]
        [Column("event_id")]
        public int EventId { get; set; }

        [Column("season_id")]
        public int SeasonId { get; set; }

        [Column("gp_name")]
        public required string GpName { get; set; }

        [Column("country_name")]
        public required string CountryName { get; set; }

        [Column("date_start")]
        public DateTime DateStart { get; set; }
    }
}
