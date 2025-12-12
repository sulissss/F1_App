using System;
using System.Collections.Generic;

namespace F1App.Models
{
    public class TeamDetailsViewModel
    {
        public int TeamId { get; set; }
        public required string TeamName { get; set; }
        public int Year { get; set; }
        public List<TeamRaceResultViewModel> Results { get; set; } = new();

        // UI Helpers
        public string? TeamLogoUrl { get; set; }
        public string? CarImageUrl { get; set; }
    }

    public class TeamRaceResultViewModel
    {
        public required string GpName { get; set; }
        public int SessionId { get; set; }
        public DateTime Date { get; set; }
        public decimal Points { get; set; }
        public string? CountryFlagUrl { get; set; }
    }
}
