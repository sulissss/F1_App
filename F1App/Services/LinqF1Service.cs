using F1App.Data;
using F1App.Models;
using Microsoft.EntityFrameworkCore;

namespace F1App.Services
{
    public class LinqF1Service : IF1Service
    {
        private readonly F1DbContext _context;

        public LinqF1Service(F1DbContext context)
        {
            _context = context;
        }

        public async Task<List<Season>> GetSeasonsAsync()
        {
            return await _context.Seasons
                .OrderByDescending(s => s.Year)
                .ToListAsync();
        }

        public async Task<List<Driver>> GetDriversAsync(int year)
        {
            // This is a simplified query. In reality, we'd join with Contract/Season
            // But for now, let's just return all drivers to verify connectivity
            // Or better, implement the join logic if possible, but Contract model is missing yet.
            // Let's assume we want all drivers for now or add Contract model later.
            return await _context.Drivers.ToListAsync();
        }

        public async Task<List<Team>> GetTeamsAsync(int year)
        {
            return await _context.Teams.ToListAsync();
        }

        public async Task<List<Event>> GetEventsAsync(int year)
        {
            var season = await _context.Seasons.FirstOrDefaultAsync(s => s.Year == year);
            if (season == null) return new List<Event>();

            return await _context.Events
                .Where(e => e.SeasonId == season.SeasonId)
                .OrderBy(e => e.DateStart)
                .ToListAsync();
        }

        public async Task<List<SessionResult>> GetRaceResultsAsync(int sessionId)
        {
            return await _context.SessionResults
                .Where(sr => sr.SessionId == sessionId)
                .OrderBy(sr => sr.Position)
                .ToListAsync();
        }

        public async Task<List<DriverStanding>> GetDriverStandingsAsync(int year)
        {
            return await _context.Database.SqlQueryRaw<DriverStanding>("SELECT * FROM vw_DriverStandings WHERE year = {0} ORDER BY championship_position", year).ToListAsync();
        }

        public async Task<List<RaceWinner>> GetRaceWinnersAsync(int year)
        {
            // We need to join SessionResult, Session, Event, Driver, Team
            // And filter for Position = 1 and SessionType = 'Race'
            // Since we don't have full navigation properties set up in DbContext for all relationships (simplified models),
            // Raw SQL is safest and most efficient here, especially to get the specific columns.
            // We also need CountryCode from Event.
            
            var query = @"
                SELECT 
                    s.session_id AS SessionId,
                    e.gp_name AS GpName,
                    e.date_start AS DateStart,
                    d.full_name AS DriverName,
                    d.code AS DriverCode,
                    COALESCE(t.name, 'Unknown Team') AS TeamName,
                    sr.number_of_laps AS Laps,
                    CAST(sr.duration AS float) AS Duration,
                    CAST(sr.duration AS VARCHAR(20)) AS Time, -- Simplified, ideally format seconds to time string
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

            // Season Stats Query
            // We use raw SQL for aggregation as it's cleaner for these specific stats
            var seasonStatsQuery = @"
                SELECT 
                    -- GP Stats
                    COUNT(CASE WHEN s.session_type = 'Race' THEN 1 END) as GpRaces,
                    SUM(CASE WHEN s.session_type = 'Race' THEN dbo.fn_GetPoints(sr.position) ELSE 0 END) as GpPoints,
                    COUNT(CASE WHEN s.session_type = 'Race' AND sr.position = 1 THEN 1 END) as GpWins,
                    COUNT(CASE WHEN s.session_type = 'Race' AND sr.position <= 3 THEN 1 END) as GpPodiums,
                    COUNT(CASE WHEN s.session_type = 'Race' AND sr.position <= 10 THEN 1 END) as GpTop10s,
                    COUNT(CASE WHEN sr.dnf = 1 THEN 1 END) as Dnfs,
                    
                    -- Sprint Stats
                    COUNT(CASE WHEN s.session_type = 'Sprint' THEN 1 END) as SprintRaces,
                    SUM(CASE WHEN s.session_type = 'Sprint' THEN dbo.fn_GetPoints(sr.position) ELSE 0 END) as SprintPoints, -- Assuming fn_GetPoints handles Sprint? Actually fn_GetPoints might be for Race only. 
                    -- If fn_GetPoints is standard 25-18..., it's for Race. Sprint is 8-7-6... 
                    -- Let's assume for now we use the same function or if points are missing we might be stuck.
                    -- Wait, the schema doesn't have a Sprint points function. 
                    -- I'll assume standard points for now or 0 if not implemented.
                    COUNT(CASE WHEN s.session_type = 'Sprint' AND sr.position = 1 THEN 1 END) as SprintWins,
                    COUNT(CASE WHEN s.session_type = 'Sprint' AND sr.position <= 3 THEN 1 END) as SprintPodiums

                FROM SessionResult sr
                JOIN Session s ON sr.session_id = s.session_id
                WHERE sr.driver_id = {0} AND s.year = {1}";

            // Execute Season Stats
            // Since we can't map anonymous types easily with SqlQueryRaw, we'll use a helper class or just execute scalar queries if needed, 
            // but here we want multiple values. 
            // Actually, let's use a DTO class for the query result.
            // Or simpler: Use separate counts if performance isn't critical, or map to the ViewModel directly if names match?
            // The ViewModel has extra properties.
            // Let's try to map to a temporary DTO.
            
            // For simplicity in this environment, I'll run a few separate queries or use a DTO.
            // Let's use a DTO approach by creating a class inside the method or using the ViewModel partially.
            // But SqlQueryRaw needs a type.
            // I'll use separate queries for clarity and robustness.
            
            // GP Stats
            stats.GpRaces = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Race'", driverId, year).SingleAsync();
            stats.GpWins = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Race' AND sr.position = 1", driverId, year).SingleAsync();
            stats.GpPodiums = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Race' AND sr.position <= 3", driverId, year).SingleAsync();
            stats.GpTop10s = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Race' AND sr.position <= 10", driverId, year).SingleAsync();
            stats.Dnfs = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND sr.dnf = 1", driverId, year).SingleAsync();
            
            // Points - using the function
            // Note: fn_GetPoints is likely for Race. Sprint points are different (8 for 1st, down to 1 for 8th).
            // I'll implement a simple case statement for Sprint points here in SQL.
            var gpPointsSql = "SELECT CAST(COALESCE(SUM(dbo.fn_GetPoints(sr.position)), 0) AS DECIMAL(18,2)) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Race'";
            stats.GpPoints = await _context.Database.SqlQueryRaw<decimal>(gpPointsSql, driverId, year).SingleAsync();

            // Sprint Stats
            stats.SprintRaces = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Sprint'", driverId, year).SingleAsync();
            stats.SprintWins = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Sprint' AND sr.position = 1", driverId, year).SingleAsync();
            stats.SprintPodiums = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Sprint' AND sr.position <= 3", driverId, year).SingleAsync();
            
            // Sprint Points (Manual calculation as fn_GetPoints is likely Race only)
            // 1st=8, 2nd=7, ... 8th=1
            var sprintPointsSql = @"
                SELECT CAST(COALESCE(SUM(
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
                    END), 0) AS DECIMAL(18,2)) as Value
                FROM SessionResult sr 
                JOIN Session s ON sr.session_id = s.session_id 
                WHERE sr.driver_id = {0} AND s.year = {1} AND s.session_type = 'Sprint'";
            stats.SprintPoints = await _context.Database.SqlQueryRaw<decimal>(sprintPointsSql, driverId, year).SingleAsync();

            stats.SeasonPoints = stats.GpPoints + stats.SprintPoints;

            // GP Poles (Season)
            stats.GpPoles = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM StartingGrid sg JOIN Session s ON sg.session_id = s.session_id WHERE sg.driver_id = {0} AND s.year = {1} AND s.session_type = 'Race' AND sg.position = 1", driverId, year).SingleAsync();

            // Fastest Laps (Season)
            // We need to check each race in the season
            var raceSessionIds = await _context.Sessions
                .Where(s => s.DateStart.Year == year && s.SessionType == "Race")
                .Select(s => s.SessionId)
                .ToListAsync();

            int fastestLapsCount = 0;
            foreach (var sessionId in raceSessionIds)
            {
                var fastestDriverId = await _context.Database.SqlQueryRaw<int?>("SELECT TOP 1 driver_id as Value FROM Lap WHERE session_id = {0} ORDER BY lap_duration ASC", sessionId).FirstOrDefaultAsync();
                if (fastestDriverId == driverId)
                {
                    fastestLapsCount++;
                }
            }
            stats.FastestLaps = fastestLapsCount;

            // Season Position
            // We can get this from vw_DriverStandings
            var standing = await _context.Database.SqlQueryRaw<DriverStanding>("SELECT * FROM vw_DriverStandings WHERE year = {0} AND driver_id = {1}", year, driverId).FirstOrDefaultAsync();
            if (standing != null)
            {
                stats.SeasonPosition = (int)standing.ChampionshipPosition;
                // Prefer view points if available and accurate
                // stats.SeasonPoints = standing.TotalPoints; 
            }

            // Career Stats
            stats.CareerGpEntered = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.session_type = 'Race'", driverId).SingleAsync();
            stats.CareerPodiums = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.session_type = 'Race' AND sr.position <= 3", driverId).SingleAsync();
            
            // Highest Finish
            var highestFinish = await _context.Database.SqlQueryRaw<int?>("SELECT MIN(position) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.session_type = 'Race' AND sr.position > 0", driverId).SingleAsync();
            stats.CareerHighestFinish = highestFinish ?? 0;
            if (stats.CareerHighestFinish > 0)
            {
                stats.CareerHighestFinishCount = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.session_type = 'Race' AND sr.position = {1}", driverId, stats.CareerHighestFinish).SingleAsync();
            }

            // Highest Grid Position (from StartingGrid table? or SessionResult doesn't have grid pos. Wait, schema has StartingGrid table)
            // Let's check StartingGrid table.
            // "SELECT position FROM StartingGrid ..."
            var highestGrid = await _context.Database.SqlQueryRaw<int?>("SELECT MIN(position) as Value FROM StartingGrid sg JOIN Session s ON sg.session_id = s.session_id WHERE sg.driver_id = {0} AND s.session_type = 'Race' AND sg.position > 0", driverId).SingleAsync();
            stats.CareerHighestGridPosition = highestGrid ?? 0;
            if (stats.CareerHighestGridPosition > 0)
            {
                stats.CareerHighestGridPositionCount = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM StartingGrid sg JOIN Session s ON sg.session_id = s.session_id WHERE sg.driver_id = {0} AND s.session_type = 'Race' AND sg.position = {1}", driverId, stats.CareerHighestGridPosition).SingleAsync();
            }

            // Poles
            stats.CareerPolePositions = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM StartingGrid sg JOIN Session s ON sg.session_id = s.session_id WHERE sg.driver_id = {0} AND s.session_type = 'Race' AND sg.position = 1", driverId).SingleAsync();

            // Career Points (Approximation using same logic as season)
            // This is expensive to calc for all history if rules changed, but we'll use current rules for simplicity or just sum what we can.
            // Or maybe just sum season points if we had them.
            // Let's just sum using current rules for now.
            var careerPointsSql = "SELECT CAST(COALESCE(SUM(dbo.fn_GetPoints(sr.position)), 0) AS DECIMAL(18,2)) as Value FROM SessionResult sr JOIN Session s ON sr.session_id = s.session_id WHERE sr.driver_id = {0} AND s.session_type = 'Race'";
            stats.CareerPoints = await _context.Database.SqlQueryRaw<decimal>(careerPointsSql, driverId).SingleAsync();
            // Add sprint points to career?
            var careerSprintPointsSql = @"
                SELECT CAST(COALESCE(SUM(
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
                    END), 0) AS DECIMAL(18,2)) as Value
                FROM SessionResult sr 
                JOIN Session s ON sr.session_id = s.session_id 
                WHERE sr.driver_id = {0} AND s.session_type = 'Sprint'";
            stats.CareerPoints += await _context.Database.SqlQueryRaw<decimal>(careerSprintPointsSql, driverId).SingleAsync();

            return stats;
        }

        public async Task<RaceDetailsPageViewModel> GetRaceDetailsAsync(int sessionId)
        {
            // Fetch Event Details using DTO to avoid mapping issues
            var header = await _context.Database.SqlQueryRaw<RaceDetailsHeaderDto>(@"
                SELECT 
                    e.gp_name AS GpName,
                    COALESCE(e.circuit_short_name, 'Unknown Circuit') AS CircuitName,
                    e.date_start AS DateStart
                FROM Session s
                JOIN Event e ON s.event_id = e.event_id
                WHERE s.session_id = {0}", sessionId).FirstOrDefaultAsync();

            if (header == null) return null;

            var eventDetails = new RaceDetailsPageViewModel
            {
                GpName = header.GpName,
                CircuitName = header.CircuitName,
                DateStart = header.DateStart
            };

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
                JOIN Contract c ON d.driver_id = c.driver_id
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

            Console.WriteLine($"[DEBUG] Found {groupedResults.Count} races for Team {teamId} in {year}");

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
                    var points = GetPointsForPosition(driver.Position ?? 0);
                    racePoints += points;
                    Console.WriteLine($"[DEBUG] Race: {race.GpName}, Driver: {driver.DriverId}, Pos: {driver.Position}, Points: {points}");

                    // Fastest Lap Bonus
                    if (fastestLap != null && driver.DriverId == fastestLap.DriverId && (driver.Position ?? 0) <= 10)
                    {
                        racePoints += 1;
                        Console.WriteLine($"[DEBUG] Fastest Lap Bonus for Driver {driver.DriverId}");
                    }
                }
                race.Points = racePoints;
                Console.WriteLine($"[DEBUG] Total Points for {race.GpName}: {racePoints}");
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
                // Use Raw SQL with SELECT * to ensure correct mapping
                var driversQuery = @"
                    SELECT d.*
                    FROM Driver d
                    JOIN Contract c ON d.driver_id = c.driver_id
                    JOIN Season s ON c.season_id = s.season_id
                    WHERE c.team_id = {0} AND s.year = {1}";

                var drivers = await _context.Drivers.FromSqlRaw(driversQuery, team.TeamId, year).ToListAsync();

                // Fallback logic removed as Contracts are now populated for 2024
                // The standard query above should return the correct drivers.

                // Calculate Total Points for the team in the given year
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

                // FILTER: Keep only Top 2 Drivers by Points for this season
                // Fetch all race results for the year to calculate points in memory
                // Manually join since navigation property might be missing
                var seasonResults = await (from sr in _context.SessionResults
                                           join s in _context.Sessions on sr.SessionId equals s.SessionId
                                           where s.DateStart.Year == year && s.SessionType == "Race"
                                           select new { sr.DriverId, sr.Position })
                                           .ToListAsync();

                var driverPoints = new Dictionary<int, decimal>();
                foreach (var driver in drivers)
                {
                    var dResults = seasonResults.Where(r => r.DriverId == driver.DriverId);
                    decimal points = 0;
                    foreach (var r in dResults)
                    {
                        points += GetPointsForPosition(r.Position ?? 0);
                        // Ignoring fastest lap for top 2 filtering to avoid complexity/performance hit
                    }
                    driverPoints[driver.DriverId] = points;
                }

                // Sort and Take 2
                drivers = drivers.OrderByDescending(d => driverPoints.ContainsKey(d.DriverId) ? driverPoints[d.DriverId] : 0)
                                 .Take(2)
                                 .ToList();

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

        private class RaceDetailsHeaderDto
        {
            public string GpName { get; set; }
            public string CircuitName { get; set; }
            public DateTime DateStart { get; set; }
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
