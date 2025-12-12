using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1App.Models
{
    public class DriverStanding
    {
        [Column("year")]
        public int Year { get; set; }

        [Column("driver_id")]
        public int DriverId { get; set; }

        [Column("full_name")]
        public required string FullName { get; set; }

        [Column("code")]
        public required string Code { get; set; }

        [Column("total_points")]
        public int TotalPoints { get; set; }

        [Column("total_wins")]
        public int TotalWins { get; set; }

        [Column("championship_position")]
        public long ChampionshipPosition { get; set; }
    }
}
