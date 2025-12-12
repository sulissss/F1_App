using System;

namespace F1App.Models
{
    public class RaceDetailsViewModel
    {
        public int Position { get; set; }
        public int DriverNumber { get; set; }
        public string DriverName { get; set; }
        public string DriverCode { get; set; }
        public string TeamName { get; set; }
        public int Laps { get; set; }
        public string TimeOrRetired { get; set; }
        public decimal Points { get; set; }
        
        // UI Helpers
        public string? DriverHeadshotUrl { get; set; }
        public string? TeamLogoUrl { get; set; }
    }

    public class RaceDetailsPageViewModel
    {
        public string GpName { get; set; }
        public string CircuitName { get; set; }
        public DateTime DateStart { get; set; }
        public List<RaceDetailsViewModel> Results { get; set; } = new List<RaceDetailsViewModel>();
    }
}
