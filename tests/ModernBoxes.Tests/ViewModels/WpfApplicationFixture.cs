using System.Windows;

namespace ModernBoxes.Tests.ViewModels;

public sealed class WpfApplicationFixture : IDisposable
{
    private static readonly object Lock = new();
    private static bool _initialized;

    public WpfApplicationFixture()
    {
        lock (Lock)
        {
            if (_initialized)
                return;

            if (Application.Current == null)
                new Application();

            _initialized = true;
        }
    }

    public void Dispose() { }
}
