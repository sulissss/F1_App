using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1App.Models
{
    public class RaceWinner
    {
        public int SessionId { get; set; }
        public required string GpName { get; set; }
        public DateTime DateStart { get; set; }
        public required string DriverName { get; set; }
        public required string DriverCode { get; set; }
        public required string TeamName { get; set; }
        public int Laps { get; set; }
        public double? Duration { get; set; } // Raw duration in seconds
        public required string Time { get; set; } // GapToLeader or Duration formatted
        public required string CountryCode { get; set; } // For flag
    }
}
