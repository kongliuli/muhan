using System;
using System.Windows;
using Velopack;

namespace ModernBoxes
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            VelopackApp.Build()
                .SetArgs(args)
                .Run();

            ModernBoxes.Desktop.StartupContext.Args = args;
            ModernBoxes.Desktop.StartupContext.Gate =
                new ModernBoxes.Infrastructure.Platform.SingleInstanceGate();

            if (!ModernBoxes.Desktop.StartupContext.Gate.IsFirstInstance)
            {
                ModernBoxes.Desktop.StartupContext.Gate.TryForwardArguments(args);
                ModernBoxes.Desktop.StartupContext.Gate.Dispose();
                return;
            }

            try
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
            finally
            {
                ModernBoxes.Desktop.StartupContext.Gate?.Dispose();
                ModernBoxes.Desktop.StartupContext.Gate = null;
            }
        }
    }
}
