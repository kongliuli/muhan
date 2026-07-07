using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Core.Enums;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.Views;
using ModernBoxes.Presentation.Dialogs;
using ModernBoxes.Presentation.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace ModernBoxes
{
    /// <summary>
    /// 主窗口：无边框侧边栏 + 左右卡片面板 + 托盘
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 卡片面板布局方向
        /// </summary>
        private CommentLayout commentLayout = CommentLayout.right;

        public MainWindow()
        {
            InitializeComponent();

            if (ConfigHelper.getConfig("x") != String.Empty)
            {
                // 上次固定过位置，按记录的位置显示
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = Convert.ToInt32(ConfigHelper.getConfig("x"));
                this.Top = Convert.ToInt32(ConfigHelper.getConfig("y"));
            }
            this.SourceInitialized += MainWindow_SourceInitialized;
            ChangeTheme.ThemeChanged += OnThemeChanged;
            this.window.MouseLeftButtonDown += Window_MouseLeftButtonDown;
            this.Height = SystemParameters.PrimaryScreenHeight - 70;
            // 不在任务栏显示图标
            this.ShowInTaskbar = false;

            // ViewModel 消息注册
            WeakReferenceMessenger.Default.Register<Boolean>(this, "isShow", ShowCardApplaction);
            WeakReferenceMessenger.Default.Register<CloseComponentMessage>(this, (r, m) => CloseComponentLayout());
            WeakReferenceMessenger.Default.Register<SetWindowOpacityMessage>(this, (r, m) => this.Opacity = m.Opacity);
            WeakReferenceMessenger.Default.Register<SetComponentWidthMessage>(this, (r, m) => SetComponentWidth(m.Width));
            WeakReferenceMessenger.Default.Register<GetComponentWidthRequest>(this, (r, m) => m.Reply(componentLayoutLeft.Width));

            loadComment();

            // 应用主题
            var themeStr = ConfigHelper.getConfig("theme");
            if (Enum.TryParse<Theme>(themeStr, out var theme))
                ChangeTheme.SetTheme(theme);

            // 恢复卡片面板宽度
            if (Double.TryParse(ConfigHelper.getConfig("ComponentWidth"), out var componentWidth))
                SetComponentWidth(componentWidth);

            // 恢复窗口透明度
            if (Double.TryParse(ConfigHelper.getConfig("WindowOpacity"), out var opacity))
                this.Opacity = opacity;
        }

        private void SetComponentWidth(double value)
        {
            componentLayoutLeft.Width = value;
            componentLayoutRight.Width = value;
        }

        /// <summary>
        /// 按配置加载卡片面板（左侧或右侧）
        /// </summary>
        private void loadComment()
        {
            var layoutStr = ConfigHelper.GetComponentLayout();
            if (string.IsNullOrEmpty(layoutStr))
                layoutStr = CommentLayout.right.ToString();
            commentLayout = (CommentLayout)Enum.Parse(typeof(CommentLayout), layoutStr);
            if (commentLayout == CommentLayout.left)
            {
                componentLayoutLeft.Content = new UcComponent();
            }
            else
            {
                componentLayoutRight.Content = new UcComponent();
            }
        }

        /// <summary>
        /// 显示/隐藏卡片面板
        /// </summary>
        public void ShowCardApplaction(Boolean isShow)
        {
            if (commentLayout == CommentLayout.right)
            {
                componentLayoutRight.Visibility = (componentLayoutRight.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
            }
            else
            {
                componentLayoutLeft.Visibility = (componentLayoutLeft.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
            }

            InvalidateMeasure();
        }

        /// <summary>
        /// 关闭卡片面板并按配置重新加载布局
        /// </summary>
        public void CloseComponentLayout()
        {
            componentLayoutLeft.Visibility = Visibility.Collapsed;
            componentLayoutRight.Visibility = Visibility.Collapsed;
            loadComment();
        }

        /// <summary>
        /// 固定/取消固定窗口
        /// </summary>
        private void MenuItem_Click_To_Fixed(object sender, RoutedEventArgs e)
        {
            if (btn_fixed.IsChecked)
            {
                this.window.MouseLeftButtonDown += Window_MouseLeftButtonDown;
                btn_fixed.IsChecked = false;
            }
            else
            {
                // 固定时记录窗口位置
                ConfigHelper.setConfig("x", this.Left);
                ConfigHelper.setConfig("y", this.Top);
                this.window.MouseLeftButtonDown -= Window_MouseLeftButtonDown;
                btn_fixed.IsChecked = true;
            }
        }

        /// <summary>
        /// 窗口拖拽
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 窗口置顶
        /// </summary>
        private void topItem_Click(object sender, RoutedEventArgs e)
        {
            topItem.IsChecked = true;
            BottomItem.IsChecked = false;
            this.Topmost = true;
        }

        /// <summary>
        /// 窗口置底
        /// </summary>
        private void BottomItem_Click(object sender, RoutedEventArgs e)
        {
            topItem.IsChecked = false;
            BottomItem.IsChecked = true;
            this.Topmost = false;
        }

        /// <summary>
        /// 关闭窗口（实际隐藏，见 OnClosing）
        /// </summary>
        private void MenuItem_Click_To_CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 拦截关闭：隐藏窗口而不退出
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        /// <summary>
        /// 打开添加卡片应用的对话框
        /// </summary>
        private void AddCardApp_Click(object sender, RoutedEventArgs e)
        {
            BaseDialog baseDialog = new BaseDialog();
            baseDialog.SetTitle("添加卡片应用");
            baseDialog.setDialogSize(565, 400);
            baseDialog.SetContent(new UcAddCardApplicationDialog());
            baseDialog.ShowDialog();
        }

        /// <summary>
        /// 删除菜单项
        /// </summary>
        private void BtnDeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.CommandParameter is string menuName)
            {
                WeakReferenceMessenger.Default.Send(new DeleteMenuItemMessage(menuName));
                WeakReferenceMessenger.Default.Send(new RefreshMenuMessage());
            }
        }

        private void ShowHide_Click(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.Hide();
                if (sender is MenuItem menuItem)
                    menuItem.Header = "显示主窗口";
            }
            else
            {
                this.Show();
                this.Activate();
                this.WindowState = WindowState.Normal;
                if (sender is MenuItem menuItem)
                    menuItem.Header = "隐藏主窗口";
            }
        }

        private void QuickNote_Click(object sender, RoutedEventArgs e)
        {
            BaseDialog baseDialog = new BaseDialog();
            baseDialog.SetTitle("新建便签");
            baseDialog.SetHeight(380);
            baseDialog.SetContent(new AddNoteDialog());
            baseDialog.ShowDialog();
        }

        private void Backup_Click(object sender, RoutedEventArgs e)
        {
            App.AppHost!.Services.GetRequiredService<BackupViewModel>().ExportData();
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            App.AppHost!.Services.GetRequiredService<BackupViewModel>().ImportData();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            BaseDialog dialog = new BaseDialog();
            dialog.SetTitle("设置");
            dialog.setDialogSize(560, 800);
            dialog.SetContent(new UCSetDialog());
            dialog.ShowDialog();
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            Win32Helper.EnableMaterial(handle);
            ApplyMaterialBrush();

            HotkeyManager.Instance.Initialize(this);
        }

        private void OnThemeChanged()
        {
            Dispatcher.Invoke(() =>
            {
                var handle = new WindowInteropHelper(this).Handle;
                Win32Helper.EnableMaterial(handle);
                ApplyMaterialBrush();
            });
        }

        private void ApplyMaterialBrush()
        {
            var theme = ConfigHelper.getConfig("theme");
            var brushKey = theme == "light" ? "MaterialBackgroundLight" : "MaterialBackground";
            if (System.Windows.Application.Current.Resources[brushKey] is SolidColorBrush brush)
            {
                System.Windows.Application.Current.Resources["MaterialBackground"] = brush.Clone();
            }
        }
    }
}
