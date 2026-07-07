using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ModernBoxes.Infrastructure
{
    public static class GlobalExceptionHandler
    {
        public static void Register()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            System.Windows.Application.Current.DispatcherUnhandledException += OnDispatcherException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Logger.Fatal(ex, "AppDomain unhandled exception");
            ShowError(ex, e.IsTerminating);
        }

        private static void OnDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Error(e.Exception, "Dispatcher unhandled exception");
            e.Handled = true;
            ShowError(e.Exception, false);
        }

        private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Logger.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        }

        private static void ShowError(Exception? ex, bool isFatal)
        {
            string msg = isFatal
                ? $"致命错误：应用程序即将关闭。\n{ex?.Message}"
                : $"发生错误：{ex?.Message}\n\n详细信息已记录到日志文件。";
            MessageBox.Show(msg, "ModernBoxes - 错误",
                MessageBoxButton.OK, isFatal ? MessageBoxImage.Error : MessageBoxImage.Warning);
        }
    }
}
