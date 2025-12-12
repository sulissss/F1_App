using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1App.Models
{
    public class SessionResult
    {
        [Column("session_id")]
        public int SessionId { get; set; }

        [Column("driver_id")]
        public int DriverId { get; set; }

        [Column("position")]
        public int? Position { get; set; }

        [Column("number_of_laps")]
        public int NumberOfLaps { get; set; }

        [Column("gap_to_leader")]
        public string? GapToLeader { get; set; }

        [Column("dnf")]
        public bool Dnf { get; set; }
    }
}
