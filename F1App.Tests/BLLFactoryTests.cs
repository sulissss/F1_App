using Xunit;
using Moq;
using F1App.Services;
using F1App.Data;
using F1App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace F1App.Tests
{
    public class BLLFactoryTests
    {
        [Fact]
        public void GetService_WhenUseStoredProceduresFalse_ReturnsLinqService()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new DbContextOptionsBuilder<F1DbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_LinqFactory")
                .Options;
            services.AddScoped(_ => new F1DbContext(options));
            services.AddScoped<LinqF1Service>();
            services.AddScoped<StoredProcF1Service>();
            var serviceProvider = services.BuildServiceProvider();
            var config = new ServiceConfig { UseStoredProcedures = false };
            var factory = new BLLFactory(serviceProvider, config);

            // Act
            var service = factory.GetService();

            // Assert
            Assert.IsType<LinqF1Service>(service);
        }

        [Fact]
        public void GetService_WhenUseStoredProceduresTrue_ReturnsStoredProcService()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new DbContextOptionsBuilder<F1DbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_SPFactory")
                .Options;
            services.AddScoped(_ => new F1DbContext(options));
            services.AddScoped<LinqF1Service>();
            services.AddScoped<StoredProcF1Service>();
            var serviceProvider = services.BuildServiceProvider();
            var config = new ServiceConfig { UseStoredProcedures = true };
            var factory = new BLLFactory(serviceProvider, config);

            // Act
            var service = factory.GetService();

            // Assert
            Assert.IsType<StoredProcF1Service>(service);
        }
    }

    public class ServiceConfigTests
    {
        [Fact]
        public void UseStoredProcedures_DefaultIsFalse()
        {
            // Arrange & Act
            var config = new ServiceConfig();

            // Assert
            Assert.False(config.UseStoredProcedures);
        }

        [Fact]
        public void UseStoredProcedures_CanBeSetToTrue()
        {
            // Arrange
            var config = new ServiceConfig();

            // Act
            config.UseStoredProcedures = true;

            // Assert
            Assert.True(config.UseStoredProcedures);
        }
    }
}
