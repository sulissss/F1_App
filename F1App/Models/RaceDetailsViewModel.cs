using System;

namespace F1App.Models
{
    public class RaceDetailsViewModel
    {
        public int Position { get; set; }
        public int DriverNumber { get; set; }
        public required string DriverName { get; set; }
        public required string DriverCode { get; set; }
        public required string TeamName { get; set; }
        public int Laps { get; set; }
        public required string TimeOrRetired { get; set; }
        public decimal Points { get; set; }
        
        // UI Helpers
        public string? TeamLogoUrl { get; set; }
        public string? DriverHeadshotUrl { get; set; }
        public string? CountryFlagUrl { get; set; }
    }

    public class RaceDetailsPageViewModel
    {
        public required string GpName { get; set; }
        public required string CircuitName { get; set; }
        public DateTime DateStart { get; set; }
        public List<RaceDetailsViewModel> Results { get; set; } = new List<RaceDetailsViewModel>();
    }
}
