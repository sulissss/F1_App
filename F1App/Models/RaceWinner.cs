using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1App.Models
{
    public class RaceWinner
    {
        public int SessionId { get; set; }
        public string GpName { get; set; }
        public DateTime DateStart { get; set; }
        public string DriverName { get; set; }
        public string DriverCode { get; set; }
        public string TeamName { get; set; }
        public int Laps { get; set; }
        public double? Duration { get; set; } // Raw duration in seconds
        public string Time { get; set; } // GapToLeader or Duration formatted
        public string CountryCode { get; set; } // For flag
    }
}
