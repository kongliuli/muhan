using CommunityToolkit.Mvvm.ComponentModel;
using ModernBoxes.Infrastructure;
using ModernBoxes.Infrastructure.Plugins;
using ModernBoxes.Sdk.Plugins;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModernBoxes.Presentation.ViewModels;

public partial class PluginManagerViewModel : ObservableObject
{
    private readonly PluginCatalogService _catalog;
    private readonly IUserNotifier _notifier;

    public ObservableCollection<FlowStoreEntry> StorePlugins { get; } = new();

    private FlowStoreEntry? _selectedStorePlugin;
    public FlowStoreEntry? SelectedStorePlugin
    {
        get => _selectedStorePlugin;
        set => SetProperty(ref _selectedStorePlugin, value);
    }

    private string _statusMessage = "可从本机 Flow/Wox 导入，或双击商店列表项自动安装并启用。";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string LocalFlowPaths =>
        string.Join("\n", _catalog.DiscoverLocalFlowRoots().DefaultIfEmpty("（未检测到 Flow/Wox 插件目录）"));

    public RelayCommand ImportFlowCommand { get; }
    public RelayCommand RefreshStoreCommand { get; }
    public RelayCommand InstallSelectedCommand { get; }

    public PluginManagerViewModel(PluginCatalogService catalog, IUserNotifier notifier)
    {
        _catalog = catalog;
        _notifier = notifier;

        ImportFlowCommand = new RelayCommand(_ => ImportFromFlow(), _ => !IsBusy);
        RefreshStoreCommand = new RelayCommand(async _ => await RefreshStoreAsync(), _ => !IsBusy);
        InstallSelectedCommand = new RelayCommand(async _ => await InstallSelectedAsync(), _ => !IsBusy && SelectedStorePlugin != null);
    }

    public async Task InstallSelectedAsync()
    {
        if (SelectedStorePlugin == null || IsBusy)
            return;

        IsBusy = true;
        try
        {
            var result = await _catalog.InstallFromFlowStoreAsync(SelectedStorePlugin);
            StatusMessage = result.Success
                ? $"已安装并启用「{result.PluginName}」，可立即在搜索中使用。"
                : $"安装「{SelectedStorePlugin.Name}」失败。";
            if (result.Success)
                _notifier.ShowWarning("插件安装", StatusMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ImportFromFlow()
    {
        IsBusy = true;
        try
        {
            var result = _catalog.ImportFromLocalFlowInstallations();
            StatusMessage = result.Success
                ? $"已导入并启用 {result.PluginName}，可立即在搜索中使用。"
                : "未发现可导入的 C# 插件（或已全部导入）。";
            if (result.Success)
                _notifier.ShowWarning("插件导入", StatusMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshStoreAsync()
    {
        IsBusy = true;
        try
        {
            var items = await _catalog.FetchFlowStoreCatalogAsync();
            StorePlugins.Clear();
            foreach (var item in items.Take(200))
                StorePlugins.Add(item);
            StatusMessage = StorePlugins.Count > 0
                ? $"已加载 {StorePlugins.Count} 个 C# 插件；双击即可自动安装并启用。"
                : "无法加载 Flow 插件商店，请检查网络。";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
