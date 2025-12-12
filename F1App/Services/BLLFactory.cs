using Microsoft.Extensions.DependencyInjection;

namespace F1App.Services
{
    public class BLLFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceConfig _config;

        public BLLFactory(IServiceProvider serviceProvider, ServiceConfig config)
        {
            _serviceProvider = serviceProvider;
            _config = config;
        }

        public IF1Service GetService()
        {
            if (_config.UseStoredProcedures)
            {
                return _serviceProvider.GetRequiredService<StoredProcF1Service>();
            }
            else
            {
                return _serviceProvider.GetRequiredService<LinqF1Service>();
            }
        }
    }
}
