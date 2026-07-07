using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Core.Interfaces.Repositories;
using ModernBoxes.Infrastructure.Compat;
using ModernBoxes.Infrastructure;
using ModernBoxes.Infrastructure.Plugins;
using ModernBoxes.Sdk.Host;
using ModernBoxes.Sdk.Search;
using System.Collections.Generic;

namespace ModernBoxes.Infrastructure.Query
{
    public sealed class SearchPluginReloadService
    {
        private readonly SearchPluginRegistry _registry;
        private readonly IServiceProvider _services;

        public IReadOnlyList<string> LastFailures { get; private set; } = new List<string>();

        public SearchPluginReloadService(SearchPluginRegistry registry, IServiceProvider services)
        {
            _registry = registry;
            _services = services;
        }

        public void Reload()
        {
            var failures = new List<string>();
            var plugins = new List<ISearchPlugin>(CreateBuiltInPlugins());
            var api = _services.GetRequiredService<IPublicAPI>();

            plugins.AddRange(WoxPluginLoader.LoadSearchPlugins(api, AppPaths.Plugins, failures));
            plugins.AddRange(JsonRpcPluginLoader.LoadSearchPlugins(AppPaths.Plugins, failures));

            _registry.Replace(plugins);
            LastFailures = failures;
        }

        private IEnumerable<ISearchPlugin> CreateBuiltInPlugins()
        {
            yield return new MenuSearchPlugin(_services.GetRequiredService<IMenuRepository>());
            yield return new ApplicationSearchPlugin(_services.GetRequiredService<IApplicationRepository>());
            yield return new TempDirSearchPlugin(_services.GetRequiredService<ITempDirRepository>());
            yield return new TempFileSearchPlugin(_services.GetRequiredService<ITempFileRepository>());
            yield return new NoteSearchPlugin(_services.GetRequiredService<INoteRepository>());
        }
    }
}
