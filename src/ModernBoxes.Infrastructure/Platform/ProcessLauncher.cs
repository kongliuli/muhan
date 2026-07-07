using ModernBoxes.Core.Interfaces;
using System.Diagnostics;

namespace ModernBoxes.Infrastructure.Platform
{
    public sealed class ProcessLauncher : IProcessLauncher
    {
        public void Start(string fileName, bool useShellExecute = false) =>
            Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = useShellExecute });
    }
}
