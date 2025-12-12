using Xunit;
using F1App.Services;
using F1App.Data;
using F1App.Models;
using Microsoft.EntityFrameworkCore;

namespace F1App.Tests
{
    public class LinqF1ServiceTests : IDisposable
    {
        private readonly F1DbContext _context;
        private readonly LinqF1Service _service;

        public LinqF1ServiceTests()
        {
            var options = new DbContextOptionsBuilder<F1DbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new F1DbContext(options);
            _service = new LinqF1Service(_context);
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Seed Seasons
            _context.Seasons.AddRange(
                new Season { SeasonId = 1, Year = 2024 },
                new Season { SeasonId = 2, Year = 2023 }
            );

            // Seed Teams
            _context.Teams.AddRange(
                new Team { TeamId = 1, Name = "Red Bull Racing", ColourHex = "#3671C6" },
                new Team { TeamId = 2, Name = "Ferrari", ColourHex = "#E80020" }
            );

            // Seed Drivers
            _context.Drivers.AddRange(
                new Driver { DriverId = 1, Code = "VER", FullName = "Max Verstappen", DriverNumber = 1 },
                new Driver { DriverId = 2, Code = "LEC", FullName = "Charles Leclerc", DriverNumber = 16 }
            );

            // Seed Contracts
            _context.Contracts.AddRange(
                new Contract { ContractId = 1, DriverId = 1, TeamId = 1, SeasonId = 1, Driver = _context.Drivers.Local.First(d => d.DriverId == 1), Team = _context.Teams.Local.First(t => t.TeamId == 1), Season = _context.Seasons.Local.First(s => s.SeasonId == 1) },
                new Contract { ContractId = 2, DriverId = 2, TeamId = 2, SeasonId = 1, Driver = _context.Drivers.Local.First(d => d.DriverId == 2), Team = _context.Teams.Local.First(t => t.TeamId == 2), Season = _context.Seasons.Local.First(s => s.SeasonId == 1) }
            );

            // Seed Events
            _context.Events.AddRange(
                new Event { EventId = 1, SeasonId = 1, GpName = "Bahrain Grand Prix", CountryName = "Bahrain", DateStart = new DateTime(2024, 3, 2) }
            );

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetSeasonsAsync_ReturnsSeasons()
        {
            // Act
            var seasons = await _service.GetSeasonsAsync();

            // Assert
            Assert.NotEmpty(seasons);
            Assert.Equal(2, seasons.Count);
            Assert.Contains(seasons, s => s.Year == 2024);
        }

        [Fact]
        public async Task GetTeamsAsync_ReturnsTeams()
        {
            // Act
            var teams = await _service.GetTeamsAsync(2024);

            // Assert
            Assert.NotEmpty(teams);
            Assert.Contains(teams, t => t.Name == "Red Bull Racing");
        }

        [Fact]
        public async Task GetDriversAsync_ReturnsDrivers()
        {
            // Act
            var drivers = await _service.GetDriversAsync(2024);

            // Assert
            Assert.NotEmpty(drivers);
            Assert.Contains(drivers, d => d.Code == "VER");
        }

        [Fact]
        public async Task GetEventsAsync_ReturnsEvents()
        {
            // Act
            var events = await _service.GetEventsAsync(2024);

            // Assert
            Assert.NotEmpty(events);
            Assert.Contains(events, e => e.GpName == "Bahrain Grand Prix");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }

    /// <summary>
    /// Note: StoredProcF1Service uses FromSqlRaw queries which cannot be tested with InMemory database.
    /// These tests require an actual SQL Server database connection.
    /// For unit testing purposes, we verify that the service can be instantiated and
    /// that it implements the correct interface.
    /// </summary>
    public class StoredProcF1ServiceTests
    {
        [Fact]
        public void StoredProcF1Service_CanBeInstantiated()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<F1DbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new F1DbContext(options);

            // Act
            var service = new StoredProcF1Service(context);

            // Assert
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IF1Service>(service);
        }

        [Fact]
        public void StoredProcF1Service_Implements_IF1Service()
        {
            // Assert
            Assert.True(typeof(IF1Service).IsAssignableFrom(typeof(StoredProcF1Service)));
        }
    }
}
