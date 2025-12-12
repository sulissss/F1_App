namespace F1App.Models
{
    public class RaceWinnerViewModel
    {
        public int SessionId { get; set; }
        public string GpName { get; set; }
        public DateTime DateStart { get; set; }
        public string DriverName { get; set; }
        public string DriverCode { get; set; }
        public string TeamName { get; set; }
        public int Laps { get; set; }
        public string Time { get; set; }
        public string CountryCode { get; set; }

        // UI Helpers
        public string? TeamLogoUrl { get; set; }
        public string? DriverHeadshotUrl { get; set; }
        public string? CountryFlagUrl { get; set; }
    }
}
