using System.Diagnostics;

namespace CreatePdf.NET.Internal;

internal interface IProcessRunner
{
    Task RunAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken);
}
