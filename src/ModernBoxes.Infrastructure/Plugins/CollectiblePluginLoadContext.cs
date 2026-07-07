using System.Reflection;
using System.Runtime.Loader;

namespace ModernBoxes.Infrastructure.Plugins
{
    /// <summary>
    /// 每个插件独立 ALC；Sdk/Core/WPF 共享程序集回退到宿主默认上下文。
    /// </summary>
    internal sealed class CollectiblePluginLoadContext : AssemblyLoadContext
    {
        private static readonly HashSet<string> SharedAssemblyNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "ModernBoxes.Sdk",
            "ModernBoxes.Core",
            "CommunityToolkit.Mvvm",
            "PresentationFramework",
            "PresentationCore",
            "WindowsBase",
            "System.Xaml",
            "System.Windows",
        };

        private readonly AssemblyDependencyResolver _resolver;

        public CollectiblePluginLoadContext(string mainAssemblyPath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name != null && SharedAssemblyNames.Contains(assemblyName.Name))
                return null;

            var path = _resolver.ResolveAssemblyToPath(assemblyName);
            return path != null ? LoadFromAssemblyPath(path) : null;
        }
    }
}
