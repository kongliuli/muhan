using System.IO;
using System.IO.Pipes;
using System.Text;

namespace ModernBoxes.Infrastructure.Platform
{
    public sealed class SingleInstanceGate : IDisposable
    {
        public const string MutexName = "Global\\ModernBoxes.SingleInstance";
        public const string PipeName = "ModernBoxes.Deeplink";

        private readonly Mutex _mutex;
        private CancellationTokenSource? _pipeCts;

        public SingleInstanceGate()
        {
            _mutex = new Mutex(true, MutexName, out var created);
            IsFirstInstance = created;
        }

        public bool IsFirstInstance { get; }

        public bool TryForwardArguments(IEnumerable<string> args)
        {
            try
            {
                using var client = new NamedPipeClientStream(
                    ".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                client.Connect(1500);
                using var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };
                foreach (var arg in args)
                    writer.WriteLine(arg);
                writer.WriteLine();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void StartPipeServer(Action<string[]> onArguments, CancellationToken cancellationToken)
        {
            _pipeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _ = Task.Run(() => PipeLoopAsync(onArguments, _pipeCts.Token), _pipeCts.Token);
        }

        private static async Task PipeLoopAsync(Action<string[]> onArguments, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await using var server = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.In,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                try
                {
                    await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                using var reader = new StreamReader(server, Encoding.UTF8);
                var lines = new List<string>();
                while (true)
                {
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (line == null || line.Length == 0)
                        break;
                    lines.Add(line);
                }

                if (lines.Count > 0)
                    onArguments(lines.ToArray());
            }
        }

        public void Dispose()
        {
            _pipeCts?.Cancel();
            _pipeCts?.Dispose();
            if (IsFirstInstance)
            {
                try { _mutex.ReleaseMutex(); } catch { /* ponytail: already released */ }
            }
            _mutex.Dispose();
        }
    }
}
