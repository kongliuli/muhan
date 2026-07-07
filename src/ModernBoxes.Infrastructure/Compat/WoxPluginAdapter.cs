using ModernBoxes.Sdk.Host;
using ModernBoxes.Sdk.Search;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.Compat
{
    internal sealed class WoxPluginAdapter : ISearchPlugin
    {
        private readonly object _plugin;
        private readonly MethodInfo _queryMethod;
        private readonly string _name;
        private readonly string _actionKeyword;
        private readonly Assembly _pluginAssembly;
        private bool _initialized;
        private readonly object _initLock = new();

        public WoxPluginAdapter(
            object plugin,
            MethodInfo queryMethod,
            string name,
            string actionKeyword,
            Assembly pluginAssembly)
        {
            _plugin = plugin;
            _queryMethod = queryMethod;
            _name = name;
            _actionKeyword = actionKeyword;
            _pluginAssembly = pluginAssembly;
        }

        public string Name => _name;
        public string ActionKeyword => _actionKeyword;
        public int Priority => 40;

        public void EnsureInitialized(IPublicAPI api)
        {
            lock (_initLock)
            {
                if (_initialized)
                    return;

                var init = _plugin.GetType().GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
                if (init != null && init.GetParameters().Length == 1)
                    init.Invoke(_plugin, new object[] { new WoxPublicApiBridge(api) });

                _initialized = true;
            }
        }

        public Task<IReadOnlyList<PluginResult>> QueryAsync(Sdk.Search.Query query, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var woxQuery = WoxResultMapper.CreateWoxQuery(_pluginAssembly, query);
                var raw = _queryMethod.Invoke(_plugin, new[] { woxQuery });
                return WoxResultMapper.MapResults(raw);
            }, cancellationToken);
        }
    }

    internal static class WoxResultMapper
    {
        internal sealed class WoxQueryShim
        {
            public string Search { get; set; } = string.Empty;
            public string ActionKeyword { get; set; } = string.Empty;
            public string RawQuery { get; set; } = string.Empty;
            public bool IsEmpty => string.IsNullOrEmpty(Search) && string.IsNullOrEmpty(ActionKeyword);
        }

        public static object CreateWoxQuery(Assembly pluginAssembly, Sdk.Search.Query query)
        {
            var woxQueryType = pluginAssembly.GetType("Wox.Plugin.Query")
                ?? Type.GetType("Wox.Plugin.Query, Wox.Plugin");
            if (woxQueryType != null)
            {
                var ctor = woxQueryType.GetConstructor(new[] { typeof(string) });
                if (ctor != null)
                    return ctor.Invoke(new object[] { query.Raw });

                ctor = woxQueryType.GetConstructor(new[] { typeof(string), typeof(string) });
                if (ctor != null)
                    return ctor.Invoke(new object[] { query.Search, query.ActionKeyword });
            }

            return new WoxQueryShim
            {
                Search = query.Search,
                ActionKeyword = query.ActionKeyword,
                RawQuery = query.Raw,
            };
        }

        public static IReadOnlyList<PluginResult> MapResults(object? raw)
        {
            if (raw is not IEnumerable items)
                return Array.Empty<PluginResult>();

            var list = new List<PluginResult>();
            foreach (var item in items)
            {
                if (item == null)
                    continue;

                var type = item.GetType();
                list.Add(new PluginResult
                {
                    Title = GetString(type, item, "Title") ?? string.Empty,
                    SubTitle = GetString(type, item, "SubTitle") ?? string.Empty,
                    IcoPath = GetString(type, item, "IcoPath") ?? string.Empty,
                    Score = GetInt(type, item, "Score") ?? 50,
                    Action = CreateAction(type, item, "Action"),
                    ContextData = item,
                });
            }

            return list;
        }

        private static string? GetString(Type type, object target, string propertyName) =>
            type.GetProperty(propertyName)?.GetValue(target)?.ToString();

        private static int? GetInt(Type type, object target, string propertyName)
        {
            var value = type.GetProperty(propertyName)?.GetValue(target);
            return value == null ? null : Convert.ToInt32(value);
        }

        private static Func<bool>? CreateAction(Type type, object target, string propertyName)
        {
            var action = type.GetProperty(propertyName)?.GetValue(target);
            if (action == null)
                return null;

            return () =>
            {
                if (action is Func<bool> boolFunc)
                    return boolFunc();

                if (action is Delegate del)
                {
                    var result = del.DynamicInvoke(null);
                    return result is bool b ? b : true;
                }

                return true;
            };
        }
    }
}
