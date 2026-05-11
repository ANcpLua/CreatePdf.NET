using System.Diagnostics;

namespace CreatePdf.NET.Internal;

internal sealed class ProcessRunner : IProcessRunner
{
    public static ProcessRunner Instance { get; } = new();

    public async Task<ProcessResult> RunAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(startInfo);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException(
                $"Process.Start returned null for '{startInfo.FileName}' — no new process was created.");

        // Read both streams concurrently to avoid the classic "child process blocks on a full pipe" deadlock.
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var stdout = await stdoutTask.ConfigureAwait(false);
        var stderr = await stderrTask.ConfigureAwait(false);

        return new ProcessResult(process.ExitCode, stdout, stderr);
    }
}
