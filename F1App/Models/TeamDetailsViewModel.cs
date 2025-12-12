using System;
using System.Collections.Generic;

namespace F1App.Models
{
    public class TeamDetailsViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int Year { get; set; }
        public List<TeamRaceResultViewModel> Results { get; set; } = new List<TeamRaceResultViewModel>();
    }

    public class TeamRaceResultViewModel
    {
        public string GpName { get; set; }
        public int SessionId { get; set; }
        public DateTime Date { get; set; }
        public decimal Points { get; set; }
        public string CountryFlagUrl { get; set; }
    }
}
