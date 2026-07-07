using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Enums;
using ModernBoxes.Infrastructure;
using System;

namespace ModernBoxes.Presentation.ViewModels;

public partial class UCSetDialogViewModel : ObservableObject
{
    private bool _isLoading;
    private readonly HotkeyViewModel _hotkeyViewModel;
    private readonly AutoOpenSoftware _autoOpenSoftware;

    private Theme _theme = Theme.light;
    public Theme Theme
    {
        get => _theme;
        set
        {
            if (SetProperty(ref _theme, value) && !_isLoading)
            {
                ChangeTheme.SetTheme(value);
                ConfigHelper.setConfig("theme", value);
                WeakReferenceMessenger.Default.Send(true, "IsRefreshMainMenu");
            }
        }
    }

    private double _windowOpacity = 1.0;
    public double WindowOpacity
    {
        get => _windowOpacity;
        set
        {
            if (SetProperty(ref _windowOpacity, value) && !_isLoading)
            {
                WeakReferenceMessenger.Default.Send(new SetWindowOpacityMessage(value));
                ConfigHelper.setConfig("WindowOpacity", value);
            }
        }
    }

    private bool _isHover;
    public bool IsHover
    {
        get => _isHover;
        set
        {
            if (SetProperty(ref _isHover, value) && !_isLoading)
                ConfigHelper.setConfig("IsHover", value);
        }
    }

    private HoverPosition _hoverPosition = HoverPosition.LEFT;
    public HoverPosition HoverPosition
    {
        get => _hoverPosition;
        set
        {
            if (SetProperty(ref _hoverPosition, value) && !_isLoading)
                ConfigHelper.setConfig("HoverPosition", value);
        }
    }

    private double _componentWidth;
    public double ComponentWidth
    {
        get => _componentWidth;
        set
        {
            if (SetProperty(ref _componentWidth, value) && !_isLoading)
            {
                WeakReferenceMessenger.Default.Send(new SetComponentWidthMessage(value));
                ConfigHelper.setConfig("ComponentWidth", value);
            }
        }
    }

    private bool _autoStart;
    public bool AutoStart
    {
        get => _autoStart;
        set
        {
            if (SetProperty(ref _autoStart, value) && !_isLoading)
            {
                ConfigHelper.setConfig("autoOpen", value);
                _autoOpenSoftware.SetAutoStart(value);
            }
        }
    }

    private CommentLayout _commentLayoutDirection = CommentLayout.right;
    public CommentLayout CommentLayoutDirection
    {
        get => _commentLayoutDirection;
        set
        {
            if (SetProperty(ref _commentLayoutDirection, value) && !_isLoading)
            {
                ConfigHelper.SetComponentLayout(value);
                WeakReferenceMessenger.Default.Send(new CloseComponentMessage());
            }
        }
    }

    public string ShowHideHotkey
    {
        get => _hotkeyViewModel.ShowHideHotkey;
        set => _hotkeyViewModel.ShowHideHotkey = value;
    }

    public string QuickNoteHotkey
    {
        get => _hotkeyViewModel.QuickNoteHotkey;
        set => _hotkeyViewModel.QuickNoteHotkey = value;
    }

    public bool IsRecordingShowHide { get; set; }
    public bool IsRecordingQuickNote { get; set; }
    public string AboutVersion => "v3.0.0";
    public string AboutAuthor => "ModernBoxes Team";

    public bool IsThemeLight { get => _theme == Theme.light; set { if (value) Theme = Theme.light; } }
    public bool IsThemeDark { get => _theme == Theme.dark; set { if (value) Theme = Theme.dark; } }
    public bool IsHoverOpen { get => _isHover; set { if (value) IsHover = true; } }
    public bool IsHoverLeft { get => _hoverPosition == HoverPosition.LEFT; set { if (value) HoverPosition = HoverPosition.LEFT; } }
    public bool IsHoverRight { get => _hoverPosition == HoverPosition.RIGHT; set { if (value) HoverPosition = HoverPosition.RIGHT; } }
    public bool IsAutoStartYes { get => _autoStart; set { if (value) AutoStart = true; } }
    public bool IsLayoutLeft { get => _commentLayoutDirection == CommentLayout.left; set { if (value) CommentLayoutDirection = CommentLayout.left; } }
    public bool IsLayoutRight { get => _commentLayoutDirection == CommentLayout.right; set { if (value) CommentLayoutDirection = CommentLayout.right; } }

    public RelayCommand BackupDataCommand { get; }
    public RelayCommand RestoreDataCommand { get; }
    public RelayCommand SaveSettingsCommand { get; }

    public UCSetDialogViewModel(
        HotkeyViewModel hotkeyViewModel,
        BackupViewModel backupViewModel,
        AutoOpenSoftware autoOpenSoftware)
    {
        _hotkeyViewModel = hotkeyViewModel;
        _autoOpenSoftware = autoOpenSoftware;
        BackupDataCommand = new RelayCommand(_ => backupViewModel.ExportData(), _ => true);
        RestoreDataCommand = new RelayCommand(_ => backupViewModel.ImportData(), _ => true);
        SaveSettingsCommand = new RelayCommand(_ => Save(), _ => true);
        Load();
    }

    public void Load()
    {
        _isLoading = true;

        var themeStr = ConfigHelper.getConfig("theme");
        if (!string.IsNullOrEmpty(themeStr) && Enum.TryParse<Theme>(themeStr, out var theme))
            _theme = theme;

        var opacityStr = ConfigHelper.getConfig("WindowOpacity");
        if (!string.IsNullOrEmpty(opacityStr) && double.TryParse(opacityStr, out var o))
            _windowOpacity = o;

        var hoverStr = ConfigHelper.getConfig("IsHover");
        if (!string.IsNullOrEmpty(hoverStr) && bool.TryParse(hoverStr, out var h))
            _isHover = h;

        var hoverPosStr = ConfigHelper.getConfig("HoverPosition");
        if (!string.IsNullOrEmpty(hoverPosStr) && Enum.TryParse<HoverPosition>(hoverPosStr, out var hp))
            _hoverPosition = hp;

        var widthRequest = WeakReferenceMessenger.Default.Send(new GetComponentWidthRequest());
        _componentWidth = widthRequest.HasReceivedResponse ? widthRequest.Response : 235;

        var autoStr = ConfigHelper.getConfig("autoOpen");
        if (!string.IsNullOrEmpty(autoStr) && bool.TryParse(autoStr, out var a))
            _autoStart = a;

        var layoutStr = ConfigHelper.GetComponentLayout();
        if (!string.IsNullOrEmpty(layoutStr) && Enum.TryParse<CommentLayout>(layoutStr, out var layout))
            _commentLayoutDirection = layout;

        _isLoading = false;
        OnPropertyChanged(nameof(Theme));
        OnPropertyChanged(nameof(IsThemeLight));
        OnPropertyChanged(nameof(IsThemeDark));
        OnPropertyChanged(nameof(WindowOpacity));
        OnPropertyChanged(nameof(IsHover));
        OnPropertyChanged(nameof(IsHoverOpen));
        OnPropertyChanged(nameof(IsHoverLeft));
        OnPropertyChanged(nameof(IsHoverRight));
        OnPropertyChanged(nameof(HoverPosition));
        OnPropertyChanged(nameof(ComponentWidth));
        OnPropertyChanged(nameof(AutoStart));
        OnPropertyChanged(nameof(IsAutoStartYes));
        OnPropertyChanged(nameof(CommentLayoutDirection));
        OnPropertyChanged(nameof(IsLayoutLeft));
        OnPropertyChanged(nameof(IsLayoutRight));
    }

    public void Save()
    {
        ConfigHelper.setConfig("theme", Theme);
        ConfigHelper.setConfig("WindowOpacity", WindowOpacity);
        ConfigHelper.setConfig("IsHover", IsHover);
        ConfigHelper.setConfig("HoverPosition", HoverPosition);
        ConfigHelper.setConfig("ComponentWidth", ComponentWidth);
        ConfigHelper.setConfig("autoOpen", AutoStart);
        ConfigHelper.SetComponentLayout(CommentLayoutDirection);
    }
}
