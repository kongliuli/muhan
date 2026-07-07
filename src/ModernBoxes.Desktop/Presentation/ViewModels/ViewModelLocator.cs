using System;
using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.ViewModels;

namespace ModernBoxes.Presentation.ViewModels;

public class ViewModelLocator
{
    public MainViewModel MainViewModel => App.AppHost!.Services.GetRequiredService<MainViewModel>();
    public UCusedApplicationViewModel UCusedApplicationViewModel => App.AppHost!.Services.GetRequiredService<UCusedApplicationViewModel>();
    public UCTempDirectoryViewModel UCTempDirectoryViewModel => App.AppHost!.Services.GetRequiredService<UCTempDirectoryViewModel>();
    public UctempFileViewModel UctempFileViewModel => App.AppHost!.Services.GetRequiredService<UctempFileViewModel>();
    public UCnotesViewModel UCnotesViewModel => App.AppHost!.Services.GetRequiredService<UCnotesViewModel>();
    public UcComponentViewModel UcComponentViewModel => App.AppHost!.Services.GetRequiredService<UcComponentViewModel>();
    public OneWordViewModel OneWordViewModel => App.AppHost!.Services.GetRequiredService<OneWordViewModel>();
    public UCSetDialogViewModel UCSetDialogViewModel => App.AppHost!.Services.GetRequiredService<UCSetDialogViewModel>();
    public SearchViewModel SearchViewModel => App.AppHost!.Services.GetRequiredService<SearchViewModel>();
    public BackupViewModel BackupViewModel => App.AppHost!.Services.GetRequiredService<BackupViewModel>();
    public HotkeyViewModel HotkeyViewModel => App.AppHost!.Services.GetRequiredService<HotkeyViewModel>();
    public AddMenuDialogViewModel AddMenuDialogViewModel => App.AppHost!.Services.GetRequiredService<AddMenuDialogViewModel>();
    public UCAddApplicationDialogViewModel UCAddApplicationDialogViewModel => App.AppHost!.Services.GetRequiredService<UCAddApplicationDialogViewModel>();
    public AddTempDirViewModel AddTempDirViewModel => App.AppHost!.Services.GetRequiredService<AddTempDirViewModel>();
    public AddTempFileDialogViewModel AddTempFileDialogViewModel => App.AppHost!.Services.GetRequiredService<AddTempFileDialogViewModel>();
    public UcAddCardAppDialogViewModel UcAddCardAppDialogViewModel => App.AppHost!.Services.GetRequiredService<UcAddCardAppDialogViewModel>();
    public UcManagerCardAppViewModel UcManagerCardAppViewModel => App.AppHost!.Services.GetRequiredService<UcManagerCardAppViewModel>();
    public PluginManagerViewModel PluginManagerViewModel => App.AppHost!.Services.GetRequiredService<PluginManagerViewModel>();
    public AddNoteDialogViewModel AddNoteDialogViewModel => App.AppHost!.Services.GetRequiredService<AddNoteDialogViewModel>();
}
