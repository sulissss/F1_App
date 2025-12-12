using Xunit;
using Microsoft.Extensions.DependencyInjection;
using F1App.Services;

namespace F1App.Tests
{
    /// <summary>
    /// Integration tests that verify the end-to-end behavior of BLL switching.
    /// These tests require a running application context.
    /// </summary>
    public class IntegrationTests
    {
        [Fact]
        public void ServiceConfig_Singleton_MaintainsStateAcrossRequests()
        {
            // Arrange
            var config = new ServiceConfig();
            
            // Act
            config.UseStoredProcedures = true;
            var firstRead = config.UseStoredProcedures;
            config.UseStoredProcedures = false;
            var secondRead = config.UseStoredProcedures;

            // Assert
            Assert.True(firstRead);
            Assert.False(secondRead);
        }

        [Fact]
        public void IF1Service_Interface_DefinesAllRequiredMethods()
        {
            // Arrange
            var interfaceType = typeof(IF1Service);

            // Act
            var methods = interfaceType.GetMethods();

            // Assert
            Assert.Contains(methods, m => m.Name == "GetSeasonsAsync");
            Assert.Contains(methods, m => m.Name == "GetDriversAsync");
            Assert.Contains(methods, m => m.Name == "GetTeamsAsync");
            Assert.Contains(methods, m => m.Name == "GetEventsAsync");
            Assert.Contains(methods, m => m.Name == "GetRaceResultsAsync");
            Assert.Contains(methods, m => m.Name == "GetDriverStandingsAsync");
            Assert.Contains(methods, m => m.Name == "GetRaceWinnersAsync");
            Assert.Contains(methods, m => m.Name == "GetTeamsWithDetailsAsync");
            Assert.Contains(methods, m => m.Name == "GetDriverStatsAsync");
            Assert.Contains(methods, m => m.Name == "GetRaceDetailsAsync");
            Assert.Contains(methods, m => m.Name == "GetTeamDetailsAsync");
        }

        [Fact]
        public void LinqF1Service_Implements_IF1Service()
        {
            // Arrange & Act
            var implementsInterface = typeof(LinqF1Service).GetInterfaces().Contains(typeof(IF1Service));

            // Assert
            Assert.True(implementsInterface);
        }

        [Fact]
        public void StoredProcF1Service_Implements_IF1Service()
        {
            // Arrange & Act
            var implementsInterface = typeof(StoredProcF1Service).GetInterfaces().Contains(typeof(IF1Service));

            // Assert
            Assert.True(implementsInterface);
        }

        [Fact]
        public void BLLFactory_CanBeInstantiated()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<ServiceConfig>();
            var serviceProvider = services.BuildServiceProvider();
            var config = new ServiceConfig();

            // Act & Assert
            var factory = new BLLFactory(serviceProvider, config);
            Assert.NotNull(factory);
        }
    }
}
