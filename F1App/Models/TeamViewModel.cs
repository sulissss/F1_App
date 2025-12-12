using System.Collections.Generic;

namespace F1App.Models
{
    public class TeamViewModel
    {
        public int TeamId { get; set; }
        public required string Name { get; set; }
        public required string ColourHex { get; set; }
        public required string CarImageUrl { get; set; }
        public required string TeamLogoUrl { get; set; }
        public List<Driver> Drivers { get; set; } = new List<Driver>();
        public decimal Points { get; set; }
    }
}
