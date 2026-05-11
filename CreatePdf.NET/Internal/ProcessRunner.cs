using System.Diagnostics;

namespace CreatePdf.NET.Internal;

internal sealed class ProcessRunner : IProcessRunner
{
    public static ProcessRunner Instance { get; } = new();

    public async Task RunAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(startInfo);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException(
                $"Process.Start returned null for '{startInfo.FileName}' — no new process was created.");
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
    }
}
