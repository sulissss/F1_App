namespace F1App.Models
{
    public class DriverStatsViewModel
    {
        public required Driver Driver { get; set; }
        public required Team? Team { get; set; }
        public int Year { get; set; }

        // Season Stats
        public int SeasonPosition { get; set; }
        public decimal SeasonPoints { get; set; }
        public int GpRaces { get; set; }
        public decimal GpPoints { get; set; }
        public int GpWins { get; set; }
        public int GpPodiums { get; set; }
        public int GpPoles { get; set; } // Assuming we can calculate this
        public int GpTop10s { get; set; }
        public int FastestLaps { get; set; }
        public int Dnfs { get; set; }
        
        // Sprint Stats
        public int SprintRaces { get; set; }
        public decimal SprintPoints { get; set; }
        public int SprintWins { get; set; }
        public int SprintPodiums { get; set; }

        // Career Stats
        public int CareerGpEntered { get; set; }
        public decimal CareerPoints { get; set; }
        public int CareerHighestFinish { get; set; }
        public int CareerHighestFinishCount { get; set; } // (x9)
        public int CareerPodiums { get; set; }
        public int CareerHighestGridPosition { get; set; }
        public int CareerHighestGridPositionCount { get; set; } // (x5)
        public int CareerPolePositions { get; set; }
        public int WorldChampionships { get; set; } // Hard to calculate without historical data, might default to 0 or manual list

        // UI Helpers (Not in DB)
        public string? TeamLogoUrl { get; set; }
        public string? DriverHeadshotUrl { get; set; }
        public string? DriverNationality { get; set; }
    }
}
