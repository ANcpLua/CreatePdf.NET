using System.Runtime.CompilerServices;

namespace CreatePdf.NET.Internal;

internal static class TextWrapper
{
    private const float HelveticaAvgWidth = 556f;
    private const float CharWidthEpsilon = 0.01f;

    public static List<string> Wrap(string text, int fontSize, float maxWidth)
    {
        var lines = new List<string>();
        foreach (var line in text.Split('\n', StringSplitOptions.TrimEntries))
            WrapLine(line, fontSize, maxWidth, lines);
        return lines;
    }

    private static void WrapLine(string line, int fontSize, float maxWidth, List<string> lines)
    {
        if (string.IsNullOrEmpty(line))
        {
            lines.Add(string.Empty);
            return;
        }

        if (Measure(line, fontSize) <= maxWidth + CharWidthEpsilon)
        {
            lines.Add(line);
            return;
        }

        var span = line.AsSpan();

        while (!span.IsEmpty)
        {
            var max = FindMaxFit(span, fontSize, maxWidth);

            var cut = max;
            while (cut > 0 && !char.IsWhiteSpace(span[cut - 1]))
                cut--;

            if (cut == 0) cut = max;

            lines.Add(span[..cut].TrimEnd().ToString());
            span = span[cut..].TrimStart();
        }
    }

    private static int FindMaxFit(ReadOnlySpan<char> text, int fontSize, float maxWidth)
    {
        var charWidth = HelveticaAvgWidth * fontSize * 0.001f;

        var maxChars = (int)Math.Floor(maxWidth / charWidth);

        return Math.Clamp(maxChars, 1, text.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float Measure(ReadOnlySpan<char> text, float fontSize)
    {
        return text.Length * HelveticaAvgWidth * fontSize * 0.001f;
    }
}