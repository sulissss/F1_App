using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using F1App.Models;
using F1App.Services;
using F1App.Data;

namespace F1App.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly BLLFactory _bllFactory;
    private readonly ServiceConfig _serviceConfig;
    private readonly F1DbContext _context;

    public HomeController(ILogger<HomeController> logger, BLLFactory bllFactory, ServiceConfig serviceConfig, F1DbContext context)
    {
        _logger = logger;
        _bllFactory = bllFactory;
        _serviceConfig = serviceConfig;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var service = _bllFactory.GetService();
        var seasons = await service.GetSeasonsAsync();
        // Filter for 2023 and 2024 only as per user request
        seasons = seasons.Where(s => s.Year == 2023 || s.Year == 2024).ToList();
        return View(seasons);
    }



    public async Task<IActionResult> Drivers(int year = 2024)
    {
        // Force 2024 as per user request
        if (year != 2024) year = 2024;

        var service = _bllFactory.GetService();
        
        // 1. Get Teams (Sorted by Points, with correct Colors)
        var teams = await service.GetTeamsWithDetailsAsync(year);
        teams = teams.OrderByDescending(t => t.Points).ToList();

        // 2. Get Driver Standings (for Rank/Position)
        var standings = await service.GetDriverStandingsAsync(year);

        var listViewModel = new List<DriverStatsViewModel>();

        foreach (var team in teams)
        {
            // Sort drivers within team by their standing rank
            // We need to find the driver's rank from the standings list
            var teamDrivers = new List<DriverStatsViewModel>();

            foreach (var driver in team.Drivers)
            {
                var standing = standings.FirstOrDefault(s => s.DriverId == driver.DriverId);
                var rank = standing?.ChampionshipPosition ?? 999; // Default to low rank if not found
                var points = standing?.TotalPoints ?? 0;

                var vm = new DriverStatsViewModel
                {
                    Driver = driver,
                    Team = new Team 
                    { 
                        TeamId = team.TeamId, 
                        Name = team.Name, 
                        ColourHex = team.ColourHex // Use the rich color from TeamViewModel
                    },
                    SeasonPosition = (int)rank,
                    SeasonPoints = points,
                    Year = year,
                    // UI Helpers
                    DriverHeadshotUrl = driver.HeadshotUrl, // Already populated by GetTeamsWithDetailsAsync
                    TeamLogoUrl = team.TeamLogoUrl,
                    DriverNationality = !string.IsNullOrEmpty(driver.Nationality) ? driver.Nationality : ImageHelper.GetNationality(driver.Code)
                };
                teamDrivers.Add(vm);
            }

            // Sort by Rank
            teamDrivers = teamDrivers.OrderBy(d => d.SeasonPosition).ToList();
            listViewModel.AddRange(teamDrivers);
        }
        
        ViewBag.Year = year;
        return View(listViewModel);
    }

    public async Task<IActionResult> DriverDetails(int id)
    {
        var service = _bllFactory.GetService();
        var statsList = new List<DriverStatsViewModel>();

        // Fetch for 2024 and 2023
        foreach (var year in new[] { 2024, 2023 })
        {
            var stats = await service.GetDriverStatsAsync(id, year);
            if (stats != null)
            {
                // Populate UI Helpers
                stats.DriverHeadshotUrl = ImageHelper.GetDriverHeadshot(stats.Driver.Code);
                stats.TeamLogoUrl = ImageHelper.GetTeamLogo(stats.Team?.Name);
                stats.DriverNationality = !string.IsNullOrEmpty(stats.Driver.Nationality) ? stats.Driver.Nationality : ImageHelper.GetNationality(stats.Driver.Code);
                statsList.Add(stats);
            }
        }

        if (!statsList.Any()) return NotFound();

        return View(statsList);
    }

    public async Task<IActionResult> Teams(int year = 2024)
    {
        // User requested to treat 2025 queries as 2024 due to missing data
        if (year == 2025) year = 2024;

        var service = _bllFactory.GetService();
        var teams = await service.GetTeamsWithDetailsAsync(year);
        ViewBag.Year = year;
        return View(teams);
    }

    public async Task<IActionResult> Races()
    {
        var service = _bllFactory.GetService();
        var seasons = await service.GetSeasonsAsync();
        seasons = seasons.Where(s => s.Year == 2023 || s.Year == 2024).ToList();
        return View(seasons);
    }

    public async Task<IActionResult> RaceResults(int year)
    {
        var service = _bllFactory.GetService();
        var winners = await service.GetRaceWinnersAsync(year);
        
        var viewModels = new List<RaceWinnerViewModel>();

        foreach (var winner in winners)
        {
            var vm = new RaceWinnerViewModel
            {
                SessionId = winner.SessionId,
                GpName = winner.GpName,
                DateStart = winner.DateStart,
                DriverName = winner.DriverName,
                DriverCode = winner.DriverCode,
                TeamName = winner.TeamName,
                Laps = winner.Laps,
                Time = winner.Time,
                CountryCode = winner.CountryCode
            };

            // Fix Team Name if Unknown
            if (string.Equals(vm.TeamName, "Unknown Team", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(vm.TeamName))
            {
                vm.TeamName = ImageHelper.GetDriverTeam(winner.DriverCode);
            }

            // Format Time
            if (winner.Duration.HasValue)
            {
                var ts = TimeSpan.FromSeconds(winner.Duration.Value);
                vm.Time = ts.ToString(@"h\:mm\:ss\.fff");
            }

            // Populate Images
            vm.DriverHeadshotUrl = ImageHelper.GetDriverHeadshot(winner.DriverCode);
            vm.TeamLogoUrl = ImageHelper.GetTeamLogo(vm.TeamName);
            
            // Clean up GP Name and get Flag
            var cleanName = winner.GpName.Replace("Grand Prix", "", StringComparison.OrdinalIgnoreCase).Trim();
            vm.GpName = cleanName;
            vm.CountryFlagUrl = ImageHelper.GetCountryFlag(cleanName);
            
            viewModels.Add(vm);
        }

        ViewBag.Year = year;
        return View(viewModels);
    }

    public async Task<IActionResult> RaceDetails(int sessionId)
    {
        var service = _bllFactory.GetService();
        var vm = await service.GetRaceDetailsAsync(sessionId);
        if (vm == null) return NotFound();

        // Populate Images
        foreach (var result in vm.Results)
        {
            result.DriverHeadshotUrl = ImageHelper.GetDriverHeadshot(result.DriverCode);
            
            // Fix Team Name if Unknown
            if (string.Equals(result.TeamName, "Unknown Team", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(result.TeamName))
            {
                result.TeamName = ImageHelper.GetDriverTeam(result.DriverCode);
            }
            
            result.TeamLogoUrl = ImageHelper.GetTeamLogo(result.TeamName);
        }
        
        // Populate Flag for Header
        ViewBag.CountryFlagUrl = ImageHelper.GetCountryFlag(vm.GpName.Replace("Grand Prix", "", StringComparison.OrdinalIgnoreCase).Trim());

        return View(vm);
    }

    public async Task<IActionResult> TeamDetails(int id, int year = 2024)
    {
        var service = _bllFactory.GetService();
        var vm = await service.GetTeamDetailsAsync(id, year);
        if (vm == null) return NotFound();

        // Populate Flags
        foreach (var result in vm.Results)
        {
            var cleanName = result.GpName.Replace("Grand Prix", "", StringComparison.OrdinalIgnoreCase).Trim();
            result.CountryFlagUrl = ImageHelper.GetCountryFlag(cleanName);
        }

        ViewBag.Year = year;
        return View(vm);
    }

    public IActionResult Settings()
    {
        return View(_serviceConfig);
    }

    [HttpPost]
    public IActionResult UpdateSettings(bool useStoredProcedures)
    {
        _serviceConfig.UseStoredProcedures = useStoredProcedures;
        return RedirectToAction("Settings");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
