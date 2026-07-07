using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Infrastructure.Plugins;
using ModernBoxes.Sdk.Search;

namespace ModernBoxes.Infrastructure.Query
{
    public static class QueryServiceCollectionExtensions
    {
        public static IServiceCollection AddModernBoxesQuery(this IServiceCollection services)
        {
            services.AddSingleton<FrecencyService>();
            services.AddSingleton<SearchPluginRegistry>();
            services.AddSingleton<SearchPluginReloadService>();
            services.AddSingleton<QueryEngine>();
            return services;
        }
    }
}
