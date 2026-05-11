namespace CreatePdf.NET.Internal;

internal readonly record struct ProcessResult(int ExitCode, string StandardOutput, string StandardError);
