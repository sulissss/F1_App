namespace F1App.Models
{
    public class RaceWinnerViewModel
    {
        public int SessionId { get; set; }
        public required string GpName { get; set; }
        public DateTime DateStart { get; set; }
        public required string DriverName { get; set; }
        public required string DriverCode { get; set; }
        public required string TeamName { get; set; }
        public int Laps { get; set; }
        public required string Time { get; set; }
        public required string CountryCode { get; set; }

        // UI Helpers
        public string? TeamLogoUrl { get; set; }
        public string? DriverHeadshotUrl { get; set; }
        public string? CountryFlagUrl { get; set; }
    }
}
