using System.Diagnostics;

namespace CreatePdf.NET.Internal;

internal interface IProcessRunner
{
    Task<ProcessResult> RunAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken);
}
