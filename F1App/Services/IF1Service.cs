using F1App.Models;

namespace F1App.Services
{
    public interface IF1Service
    {
        Task<List<Season>> GetSeasonsAsync();
        Task<List<Driver>> GetDriversAsync(int year);
        Task<List<Team>> GetTeamsAsync(int year);
        Task<List<Event>> GetEventsAsync(int year);
        Task<List<SessionResult>> GetRaceResultsAsync(int sessionId);
        Task<List<DriverStanding>> GetDriverStandingsAsync(int year);
        Task<List<RaceWinner>> GetRaceWinnersAsync(int year);
        Task<List<TeamViewModel>> GetTeamsWithDetailsAsync(int year);
        Task<DriverStatsViewModel?> GetDriverStatsAsync(int driverId, int year);
        Task<RaceDetailsPageViewModel?> GetRaceDetailsAsync(int sessionId);
        Task<TeamDetailsViewModel?> GetTeamDetailsAsync(int teamId, int year);
    }
}
