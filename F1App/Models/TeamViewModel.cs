using System.Collections.Generic;

namespace F1App.Models
{
    public class TeamViewModel
    {
        public int TeamId { get; set; }
        public string Name { get; set; }
        public string ColourHex { get; set; }
        public string CarImageUrl { get; set; }
        public string TeamLogoUrl { get; set; }
        public List<Driver> Drivers { get; set; } = new List<Driver>();
        public decimal Points { get; set; }
    }
}
