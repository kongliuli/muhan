using ModernBoxes.Infrastructure.Plugins;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.Compat
{
    internal static class JsonRpcProcessClient
    {
        public static async Task<string> CallAsync(
            ProcessStartInfo startInfo,
            string requestLine,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            using var process = new Process { StartInfo = startInfo };
            if (!process.Start())
                throw new InvalidOperationException("Failed to start jsonrpc plugin process");

            await process.StandardInput.WriteLineAsync(requestLine).ConfigureAwait(false);
            await process.StandardInput.FlushAsync().ConfigureAwait(false);
            process.StandardInput.Close();

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            Task<string?> readTask = process.StandardOutput.ReadLineAsync(timeoutCts.Token).AsTask();
            var completed = await Task.WhenAny(readTask, Task.Delay(Timeout.Infinite, timeoutCts.Token))
                .ConfigureAwait(false);

            if (completed != readTask)
                throw new OperationCanceledException("jsonrpc plugin timed out");

            var line = await readTask.ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(line))
                throw new InvalidOperationException("jsonrpc plugin returned empty response");

            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch
            {
                // ponytail: 尽力回收子进程
            }

            return line;
        }

        public static ProcessStartInfo CreateStartInfo(PluginManifest manifest, string pluginDirectory)
        {
            var mainPath = Path.Combine(pluginDirectory, manifest.Main);
            if (!File.Exists(mainPath))
                throw new FileNotFoundException("Plugin main not found", mainPath);

            ProcessStartInfo info;
            if (string.Equals(manifest.Runtime, "python", StringComparison.OrdinalIgnoreCase))
            {
                info = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = Quote(mainPath),
                    WorkingDirectory = pluginDirectory,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardInputEncoding = Encoding.UTF8,
                };
            }
            else
            {
                info = new ProcessStartInfo
                {
                    FileName = mainPath,
                    WorkingDirectory = pluginDirectory,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardInputEncoding = Encoding.UTF8,
                };
            }

            return info;
        }

        private static string Quote(string path) => $"\"{path.Replace("\"", "\\\"")}\"";
    }
}
