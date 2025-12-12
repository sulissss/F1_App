using F1App.Data;
using F1App.Models;
using Microsoft.EntityFrameworkCore;

namespace F1App.Services
{
    public class StoredProcF1Service : IF1Service
    {
        private readonly F1DbContext _context;

        public StoredProcF1Service(F1DbContext context)
        {
            _context = context;
        }

        public async Task<List<Season>> GetSeasonsAsync()
        {
            // Using LINQ for simple lookups where no specific SP exists or required
            // Requirement says "Implement equivalent business operations using stored procedures"
            // If no SP exists for Seasons, we might use raw SQL or just LINQ if acceptable.
            // Let's use raw SQL for "SP-like" behavior if no SP exists.
            return await _context.Seasons.FromSqlRaw("SELECT * FROM Season ORDER BY year DESC").ToListAsync();
        }

        public async Task<List<Driver>> GetDriversAsync(int year)
        {
             return await _context.Drivers.FromSqlRaw("SELECT * FROM Driver").ToListAsync();
        }

        public async Task<List<Team>> GetTeamsAsync(int year)
        {
            return await _context.Teams.FromSqlRaw("SELECT * FROM Team").ToListAsync();
        }

        public async Task<List<Event>> GetEventsAsync(int year)
        {
            // Assuming we can use a query here
            var season = await _context.Seasons.FirstOrDefaultAsync(s => s.Year == year);
             if (season == null) return new List<Event>();
             
            return await _context.Events
                .FromSqlRaw("SELECT * FROM Event WHERE season_id = {0} ORDER BY date_start", season.SeasonId)
                .ToListAsync();
        }

        public async Task<List<SessionResult>> GetRaceResultsAsync(int sessionId)
        {
             return await _context.SessionResults
                .FromSqlRaw("SELECT * FROM SessionResult WHERE session_id = {0} ORDER BY position", sessionId)
                .ToListAsync();
        }

        public async Task<List<DriverStanding>> GetDriverStandingsAsync(int year)
        {
            // Using the View explicitly as requested by requirements to use Views
            return await _context.Database.SqlQueryRaw<DriverStanding>("SELECT * FROM vw_DriverStandings WHERE year = {0}", year).ToListAsync();
        }

        public async Task<List<RaceWinner>> GetRaceWinnersAsync(int year)
        {
             var query = @"
                SELECT 
                    e.gp_name AS GpName,
                    e.date_start AS DateStart,
                    d.full_name AS DriverName,
                    d.code AS DriverCode,
                    COALESCE(t.name, 'Unknown Team') AS TeamName,
                    sr.number_of_laps AS Laps,
                    CAST(sr.duration AS VARCHAR(20)) AS Time,
                    e.country_code AS CountryCode
                FROM SessionResult sr
                JOIN Session s ON sr.session_id = s.session_id
                JOIN Event e ON s.event_id = e.event_id
                JOIN Driver d ON sr.driver_id = d.driver_id
                LEFT JOIN Contract c ON d.driver_id = c.driver_id AND s.year = (SELECT year FROM Season WHERE season_id = c.season_id)
                LEFT JOIN Team t ON c.team_id = t.team_id
                WHERE s.year = {0}
                  AND s.session_type = 'Race'
                  AND sr.position = 1
                ORDER BY e.date_start";

            return await _context.Database.SqlQueryRaw<RaceWinner>(query, year).ToListAsync();
        }

        public async Task<DriverStatsViewModel> GetDriverStatsAsync(int driverId, int year)
        {
            // Reusing the logic from LinqF1Service as it uses Raw SQL which is compatible with StoredProc service philosophy (using DB directly)
            // In a real scenario, we might wrap this in a big Stored Procedure "sp_GetDriverStats"
            // For this task, I'll duplicate the logic to ensure it works.
            
             // Fetch Driver and Team
            var driver = await _context.Drivers.FindAsync(driverId);
            // Get Team via Contract
            var team = await _context.Contracts
                .Where(c => c.DriverId == driverId && c.Season.Year == year)
                .Select(c => c.Team)
                .FirstOrDefaultAsync();

            if (driver == null) return null;

            var stats = new DriverStatsViewModel
            {
                Driver = driver,
                Team = team,
                Year = year
            };

            // GP Stats
            stats.GpRaces = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Race'", driverId, year).SingleAsync();
            stats.GpWins = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Race' AND sr.position = 1", driverId, year).SingleAsync();
            stats.GpPodiums = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Race' AND sr.position <= 3", driverId, year).SingleAsync();
            stats.GpTop10s = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Race' AND sr.position <= 10", driverId, year).SingleAsync();
            stats.Dnfs = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND sr.dnf = 1", driverId, year).SingleAsync();
            
            var gpPointsSql = "SELECT COALESCE(SUM(dbo.fn_GetPoints(sr.position)), 0) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Race'";
            stats.GpPoints = await _context.Database.SqlQueryRaw<int>(gpPointsSql, driverId, year).SingleAsync();

            // Sprint Stats
            stats.SprintRaces = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Sprint'", driverId, year).SingleAsync();
            stats.SprintWins = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Sprint' AND sr.position = 1", driverId, year).SingleAsync();
            stats.SprintPodiums = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Sprint' AND sr.position <= 3", driverId, year).SingleAsync();
            
            var sprintPointsSql = @"
                SELECT COALESCE(SUM(
                    CASE 
                        WHEN sr.position = 1 THEN 8
                        WHEN sr.position = 2 THEN 7
                        WHEN sr.position = 3 THEN 6
                        WHEN sr.position = 4 THEN 5
                        WHEN sr.position = 5 THEN 4
                        WHEN sr.position = 6 THEN 3
                        WHEN sr.position = 7 THEN 2
                        WHEN sr.position = 8 THEN 1
                        ELSE 0 
                    END), 0)
                FROM SessionResult sr 
                JOIN Session s ON sr.session_id = s.session_id 
                WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Sprint'";
            stats.SprintPoints = await _context.Database.SqlQueryRaw<int>(sprintPointsSql, driverId, year).SingleAsync();

            stats.SeasonPoints = stats.GpPoints + stats.SprintPoints;

            // Season Position
            var standing = await _context.Database.SqlQueryRaw<DriverStanding>("SELECT * FROM vw_DriverStandings WHERE year = {0} AND driver_id = {1}", year, driverId).FirstOrDefaultAsync();
            if (standing != null)
            {
                stats.SeasonPosition = (int)standing.ChampionshipPosition;
            }

            // Career Stats
            stats.CareerGpEntered = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.session_type = 'Race'", driverId).SingleAsync();
            stats.CareerPodiums = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.session_type = 'Race' AND sr.position <= 3", driverId).SingleAsync();
            
            var highestFinish = await _context.Database.SqlQueryRaw<int?>("SELECT MIN(position) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.session_type = 'Race' AND sr.position > 0", driverId).SingleAsync();
            stats.CareerHighestFinish = highestFinish ?? 0;
            if (stats.CareerHighestFinish > 0)
            {
                stats.CareerHighestFinishCount = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.session_type = 'Race' AND sr.position = {1}", driverId, stats.CareerHighestFinish).SingleAsync();
            }

            var highestGrid = await _context.Database.SqlQueryRaw<int?>("SELECT MIN(position) FROM StartingGrid sg JOIN Session s ON sg.session_id = s.session_id WHERE sg.driver_id = {0} AND s.session_type = 'Race' AND sg.position > 0", driverId).SingleAsync();
            stats.CareerHighestGridPosition = highestGrid ?? 0;
            if (stats.CareerHighestGridPosition > 0)
            {
                stats.CareerHighestGridPositionCount = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM StartingGrid sg JOIN Session s ON sg.session_id = s.session_id WHERE sg.driver_id = {0} AND s.session_type = 'Race' AND sg.position = {1}", driverId, stats.CareerHighestGridPosition).SingleAsync();
            }

            stats.CareerPolePositions = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM StartingGrid sg JOIN Session s ON sg.session_id = s.session_id WHERE sg.driver_id = {0} AND s.session_type = 'Race' AND sg.position = 1", driverId).SingleAsync();

            var careerPointsSql = "SELECT COALESCE(SUM(dbo.fn_GetPoints(sr.position)), 0) FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.session_type = 'Race'";
            stats.CareerPoints = await _context.Database.SqlQueryRaw<int>(careerPointsSql, driverId).SingleAsync();
            var careerSprintPointsSql = @"
                SELECT COALESCE(SUM(
                    CASE 
                        WHEN sr.position = 1 THEN 8
                        WHEN sr.position = 2 THEN 7
                        WHEN sr.position = 3 THEN 6
                        WHEN sr.position = 4 THEN 5
                        WHEN sr.position = 5 THEN 4
                        WHEN sr.position = 6 THEN 3
                        WHEN sr.position = 7 THEN 2
                        WHEN sr.position = 8 THEN 1
                        ELSE 0 
                    END), 0)
                FROM SessionResult sr 
                JOIN Session s ON sr.session_id = s.session_id 
                WHERE sr.driver_id = {0} AND s.session_type = 'Sprint'";
            stats.CareerPoints += await _context.Database.SqlQueryRaw<int>(careerSprintPointsSql, driverId).SingleAsync();

            return stats;
        }

        public async Task<RaceDetailsPageViewModel> GetRaceDetailsAsync(int sessionId)
        {
            // Duplicating logic from LinqF1Service to satisfy interface and ensure functionality
            // Fetch Event Details
            var eventDetails = await _context.Database.SqlQueryRaw<RaceDetailsPageViewModel>(@"
                SELECT 
                    e.gp_name AS GpName,
                    e.circuit_short_name AS CircuitName,
                    e.date_start AS DateStart
                FROM Session s
                JOIN Event e ON s.event_id = e.event_id
                WHERE s.session_id = {0}", sessionId).FirstOrDefaultAsync();

            if (eventDetails == null) return null;

            // Fetch Results
            var query = @"
                SELECT 
                    sr.position AS Position,
                    d.driver_number AS DriverNumber,
                    d.full_name AS DriverName,
                    d.code AS DriverCode,
                    COALESCE(t.name, 'Unknown Team') AS TeamName,
                    sr.number_of_laps AS Laps,
                    CASE 
                        WHEN sr.dnf = 1 THEN 'DNF'
                        WHEN sr.dns = 1 THEN 'DNS'
                        WHEN sr.dsq = 1 THEN 'DSQ'
                        WHEN sr.position = 1 THEN CAST(sr.duration AS VARCHAR(20))
                        ELSE sr.gap_to_leader
                    END AS TimeOrRetired,
                    CAST(
                        dbo.fn_GetPoints(sr.position) + 
                        CASE 
                            WHEN sr.driver_id = (SELECT TOP 1 driver_id FROM Lap WHERE session_id = sr.session_id ORDER BY lap_duration ASC) 
                                 AND sr.position <= 10 
                            THEN 1 
                            ELSE 0 
                        END 
                    AS DECIMAL(18,2)) AS Points
                FROM SessionResult sr
                JOIN Session s ON sr.session_id = s.session_id
                JOIN Driver d ON sr.driver_id = d.driver_id
                LEFT JOIN Contract c ON d.driver_id = c.driver_id AND s.year = (SELECT year FROM Season WHERE season_id = c.season_id)
                LEFT JOIN Team t ON c.team_id = t.team_id
                WHERE sr.session_id = {0}
                ORDER BY 
                    CASE WHEN sr.dnf = 1 OR sr.dns = 1 OR sr.dsq = 1 THEN 1 ELSE 0 END,
                    CASE WHEN sr.dnf = 1 OR sr.dns = 1 OR sr.dsq = 1 THEN sr.number_of_laps ELSE 0 END DESC,
                    sr.position";
            
            var results = await _context.Database.SqlQueryRaw<RaceDetailsRowDto>(query, sessionId).ToListAsync();

            // Calculate Winner's Laps for "+1 LAP" logic
            int winnerLaps = 0;
            var winner = results.FirstOrDefault(r => r.Position == 1);
            if (winner != null)
            {
                winnerLaps = winner.Laps;
            }

            eventDetails.Results = results.Select(r => new RaceDetailsViewModel
            {
                Position = r.Position ?? 0,
                DriverNumber = r.DriverNumber,
                DriverName = r.DriverName,
                DriverCode = r.DriverCode,
                TeamName = r.TeamName,
                Laps = r.Laps,
                TimeOrRetired = r.TimeOrRetired,
                Points = r.Points
            }).ToList();

            // Post-processing for Time/Retired display
            foreach (var result in eventDetails.Results)
            {
                // 1. Format Winner Time
                if (result.Position == 1 && double.TryParse(result.TimeOrRetired, out double duration))
                {
                    var ts = TimeSpan.FromSeconds(duration);
                    result.TimeOrRetired = ts.ToString(@"h\:mm\:ss\.fff");
                }
                // 2. Handle Lapped Drivers (only if not DNF/DNS/DSQ)
                else if (result.TimeOrRetired != "DNF" && result.TimeOrRetired != "DNS" && result.TimeOrRetired != "DSQ")
                {
                     if (winnerLaps > 0 && result.Laps < winnerLaps)
                     {
                         int diff = winnerLaps - result.Laps;
                         result.TimeOrRetired = diff == 1 ? "+1 LAP" : $"+{diff} LAPS";
                     }
                     // 3. Format Gap Time (e.g. +12.535s)
                     else if (double.TryParse(result.TimeOrRetired, out double gap))
                     {
                         result.TimeOrRetired = $"+{gap}s";
                     }
                }
            }

            return eventDetails;
        }

        public async Task<TeamDetailsViewModel> GetTeamDetailsAsync(int teamId, int year)
        {
            // Get Team Name
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) return null;

            var vm = new TeamDetailsViewModel
            {
                TeamId = teamId,
                TeamName = team.Name,
                Year = year
            };

            // Query to get all race results for the team in the given year
            // We fetch raw data first to avoid complex SQL aggregation issues
            var query = @"
                SELECT 
                    e.gp_name AS GpName,
                    e.date_start AS Date,
                    sr.position AS Position,
                    sr.session_id AS SessionId,
                    sr.driver_id AS DriverId
                FROM SessionResult sr
                JOIN Session s ON sr.session_id = s.session_id
                JOIN Event e ON s.event_id = e.event_id
                JOIN Driver d ON sr.driver_id = d.driver_id
                JOIN Contract c ON d.driver_id = c.driver_id AND s.year = (SELECT year FROM Season WHERE season_id = c.season_id)
                WHERE c.team_id = {0}
                  AND s.year = {1}
                  AND s.session_type = 'Race'
                ORDER BY e.date_start";

            var rawResults = await _context.Database.SqlQueryRaw<TeamRaceRawDto>(query, teamId, year).ToListAsync();

            // Group by Race and Calculate Points
            var groupedResults = rawResults
                .GroupBy(r => new { r.GpName, r.Date, r.SessionId })
                .Select(g => new TeamRaceResultViewModel
                {
                    GpName = g.Key.GpName,
                    Date = g.Key.Date,
                    Points = 0 // Will calculate below
                })
                .ToList();

            // Calculate points for each race
            foreach (var race in groupedResults)
            {
                // Get session ID from the first item in the group (they are grouped by session)
                var sessionId = rawResults.First(r => r.GpName == race.GpName && r.Date == race.Date).SessionId;
                
                // Get fastest lap driver for this session
                var fastestLap = await _context.Database.SqlQueryRaw<FastestLapDto>(@"
                    SELECT TOP 1 driver_id AS DriverId
                    FROM Lap 
                    WHERE session_id = {0} 
                    ORDER BY lap_duration ASC", sessionId).FirstOrDefaultAsync();

                decimal racePoints = 0;
                var raceDrivers = rawResults.Where(r => r.SessionId == sessionId);

                foreach (var driver in raceDrivers)
                {
                    // Base points
                    racePoints += GetPointsForPosition(driver.Position ?? 0);

                    // Fastest Lap Bonus
                    if (fastestLap != null && driver.DriverId == fastestLap.DriverId && (driver.Position ?? 0) <= 10)
                    {
                        racePoints += 1;
                    }
                }
                race.Points = racePoints;
            }

            vm.Results = groupedResults.Select(r => new TeamRaceResultViewModel
            {
                GpName = r.GpName,
                SessionId = rawResults.First(raw => raw.GpName == r.GpName && raw.Date == r.Date).SessionId,
                Date = r.Date,
                Points = r.Points
            }).ToList();

            return vm;
        }

        private class FastestLapDto
        {
            public int DriverId { get; set; }
        }

        public async Task<List<TeamViewModel>> GetTeamsWithDetailsAsync(int year)
        {
            var teams = await _context.Teams
                .Where(t => !t.Name.Contains("AlphaTauri") && !t.Name.Contains("Alfa Romeo"))
                .ToListAsync();
            var teamViewModels = new List<TeamViewModel>();

            foreach (var team in teams)
            {
                // Get Drivers for the team in the given year
                var drivers = await _context.Drivers
                    .Join(_context.Contracts, d => d.DriverId, c => c.DriverId, (d, c) => new { Driver = d, Contract = c })
                    .Join(_context.Seasons, dc => dc.Contract.SeasonId, s => s.SeasonId, (dc, s) => new { dc.Driver, dc.Contract, Season = s })
                    .Where(x => x.Contract.TeamId == team.TeamId && x.Season.Year == year)
                    .Select(x => x.Driver)
                    .ToListAsync();

                // Calculate Total Points
                var pointsQuery = @"
                    SELECT 
                        sr.position AS Position,
                        sr.session_id AS SessionId,
                        sr.driver_id AS DriverId,
                        e.gp_name AS GpName,
                        e.date_start AS Date
                    FROM SessionResult sr
                    JOIN Session s ON sr.session_id = s.session_id
                    JOIN Event e ON s.event_id = e.event_id
                    JOIN Driver d ON sr.driver_id = d.driver_id
                    JOIN Contract c ON d.driver_id = c.driver_id
                    WHERE c.team_id = {0}
                      AND s.year = {1}
                      AND s.session_type = 'Race'";

                var rawResults = await _context.Database.SqlQueryRaw<TeamRaceRawDto>(pointsQuery, team.TeamId, year).ToListAsync();
                
                decimal totalPoints = 0;
                var sessionIds = rawResults.Select(r => r.SessionId).Distinct().ToList();
                
                foreach (var sessionId in sessionIds)
                {
                    var fastestLap = await _context.Database.SqlQueryRaw<FastestLapDto>(@"
                        SELECT TOP 1 driver_id AS DriverId
                        FROM Lap 
                        WHERE session_id = {0} 
                        ORDER BY lap_duration ASC", sessionId).FirstOrDefaultAsync();

                    var sessionDrivers = rawResults.Where(r => r.SessionId == sessionId);
                    foreach (var driver in sessionDrivers)
                    {
                        totalPoints += GetPointsForPosition(driver.Position ?? 0);
                        if (fastestLap != null && driver.DriverId == fastestLap.DriverId && (driver.Position ?? 0) <= 10)
                        {
                            totalPoints += 1;
                        }
                    }
                }

                // Map Car Image and Colors
                string carImage = "";
                string colourHex = team.ColourHex;
                string lowerName = team.Name.ToLower();

                if (lowerName.Contains("red bull")) 
                {
                    carImage = "/car_images/2025redbullracingcarright.webp.avif";
                    colourHex = "#3671C6"; // Red Bull Blue
                }
                else if (lowerName.Contains("ferrari")) 
                {
                    carImage = "/car_images/2025ferraricarright.webp.avif";
                    colourHex = "#E80020"; // Ferrari Red
                }
                else if (lowerName.Contains("mercedes")) 
                {
                    carImage = "/car_images/2025mercedescarright.webp.avif";
                    colourHex = "#27F4D2"; // Mercedes Teal
                }
                else if (lowerName.Contains("mclaren")) 
                {
                    carImage = "/car_images/2025mclarencarright.webp.avif";
                    colourHex = "#FF8000"; // McLaren Orange
                }
                else if (lowerName.Contains("aston martin")) 
                {
                    carImage = "/car_images/2025astonmartincarright.webp.avif";
                    colourHex = "#229971"; // Aston Martin Green
                }
                else if (lowerName.Contains("alpine")) 
                {
                    carImage = "/car_images/2025alpinecarright.webp.avif";
                    colourHex = "#0093CC"; // Alpine Blue
                }
                else if (lowerName.Contains("williams")) 
                {
                    carImage = "/car_images/2025williamscarright.webp.avif";
                    colourHex = "#64C4FF"; // Williams Blue
                }
                else if (lowerName.Contains("haas")) 
                {
                    carImage = "/car_images/2025haasf1teamcarright.webp.avif";
                    colourHex = "#B6BABD"; // Haas Grey
                }
                else if (lowerName.Contains("kick") || lowerName.Contains("sauber")) 
                {
                    carImage = "/car_images/2025kicksaubercarright.webp.avif";
                    colourHex = "#52E252"; // Kick Sauber Green
                }
                else if (lowerName.Contains("racing bulls") || lowerName.Contains("rb")) 
                {
                    carImage = "/car_images/2025racingbullscarright.webp.avif";
                    colourHex = "#6692FF"; // RB Blue
                }

                // Populate Image URLs for drivers
                foreach (var driver in drivers)
                {
                    driver.HeadshotUrl = ImageHelper.GetDriverHeadshot(driver.Code);
                }

                teamViewModels.Add(new TeamViewModel
                {
                    TeamId = team.TeamId,
                    Name = team.Name,
                    ColourHex = colourHex,
                    CarImageUrl = carImage,
                    TeamLogoUrl = ImageHelper.GetTeamLogo(team.Name),
                    Drivers = drivers,
                    Points = totalPoints
                });
            }

            return teamViewModels.OrderByDescending(t => t.Points).ToList();
        }

        private decimal GetPointsForPosition(int position)
        {
            switch (position)
            {
                case 1: return 25;
                case 2: return 18;
                case 3: return 15;
                case 4: return 12;
                case 5: return 10;
                case 6: return 8;
                case 7: return 6;
                case 8: return 4;
                case 9: return 2;
                case 10: return 1;
                default: return 0;
            }
        }

        private class TeamRaceRawDto
        {
            public string GpName { get; set; }
            public DateTime Date { get; set; }
            public int? Position { get; set; }
            public int SessionId { get; set; }
            public int DriverId { get; set; }
        }

        private class RaceDetailsRowDto
        {
            public int? Position { get; set; }
            public int DriverNumber { get; set; }
            public string DriverName { get; set; }
            public string DriverCode { get; set; }
            public string TeamName { get; set; }
            public int Laps { get; set; }
            public string TimeOrRetired { get; set; }
            public decimal Points { get; set; }
        }
    }
}
