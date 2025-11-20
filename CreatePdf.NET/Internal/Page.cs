using System.Globalization;
using System.Text;

namespace CreatePdf.NET.Internal;

internal sealed class Page
{
    private readonly Dye _background;
    private readonly StringBuilder _content = new();
    private readonly List<int> _imageIds = [];

    public Page(int id, Dye background)
    {
        Id = id;
        _background = background;
        _content.AppendLine(string.Create(CultureInfo.InvariantCulture,
            $"{background.R:F6} {background.G:F6} {background.B:F6} rg"));
        _content.AppendLine(string.Create(CultureInfo.InvariantCulture,
            $"0 0 {Layout.PageWidth:F2} {Layout.PageHeight:F2} re f"));
    }

    public int Id { get; }
    public bool HasImages => _imageIds.Count > 0;
    public IEnumerable<int> ImageIds => _imageIds;

    public void AddText(ReadOnlySpan<char> text, float x, float y, float size, Dye dye)
    {
        var escaped = EscapePdfString(text);
        var pdfY = Layout.PageHeight - y - size;

        if (dye.IsSimilarTo(_background))
            dye = _background.IsLight ? Dye.Black : Dye.White;

        _content.AppendLine(string.Create(CultureInfo.InvariantCulture,
            $"q BT /Helvetica {size:F2} Tf {dye.R:F6} {dye.G:F6} {dye.B:F6} rg {x:F2} {pdfY:F2} Td ({escaped}) Tj ET Q"));
    }

    public void AddImage(ImageResource image, float x, float y, float width, float height)
    {
        _imageIds.Add(image.Id);
        _content.AppendLine(string.Create(CultureInfo.InvariantCulture,
            $"q {width:F2} 0 0 {height:F2} {x:F2} {Layout.PageHeight - y - height:F2} cm /Im{image.Id} Do Q"));
    }

    public string GetContent() => SanitizeForLatin1(_content.ToString());

    private static string SanitizeForLatin1(string text)
    {
        return text
            .Replace("€", "EUR", StringComparison.Ordinal)
            .Replace("—", "-", StringComparison.Ordinal)
            .Replace("–", "-", StringComparison.Ordinal)
            .Replace('\u201C', '"')
            .Replace('\u201D', '"')
            .Replace('\u2018', '\'')
            .Replace('\u2019', '\'')
            .Replace("…", "...", StringComparison.Ordinal)
            .Replace("œ", "oe", StringComparison.Ordinal)
            .Replace("Œ", "OE", StringComparison.Ordinal)
            .Replace("→", "->", StringComparison.Ordinal)
            .Replace("←", "<-", StringComparison.Ordinal)
            .Replace("↑", "^", StringComparison.Ordinal)
            .Replace("↓", "v", StringComparison.Ordinal);
    }

    private static string EscapePdfString(ReadOnlySpan<char> text)
    {
        return text.ToString()
            .Replace("\\", @"\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
    }
}
