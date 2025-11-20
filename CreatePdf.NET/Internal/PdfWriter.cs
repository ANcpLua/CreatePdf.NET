using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace CreatePdf.NET.Internal;

internal sealed class PdfWriter : IPdfCanvas, IAsyncDisposable
{
    private static readonly Encoding Latin1 = Encoding.Latin1;
    private readonly Dye _background;
    private readonly MemoryStream _buffer = new();
    private readonly List<ImageResource> _images = [];
    private readonly Dictionary<int, long> _offsets = [];
    private readonly Stream _output;
    private readonly List<Page> _pages = [];

    private Page _currentPage = null!;
    private float _currentY;
    private int _nextId = 1;

    public PdfWriter(Stream output, Dye background)
    {
        _output = output;
        _background = background;
        StartNewPage();
    }

    public async ValueTask DisposeAsync() => await _buffer.DisposeAsync();

    public void DrawText(string text, int size, Dye dye, TextAlignment alignment)
    {
        var lines = TextWrapper.Wrap(text, size, Layout.ContentWidth);

        foreach (var line in lines)
        {
            var height = size * Layout.LineSpacing;
            if (_currentY + height > Layout.PageHeight - Layout.Margin + 0.01f)
                StartNewPage();

            var width = TextWrapper.Measure(line, size);
            var x = alignment switch
            {
                TextAlignment.Center => Layout.PageWidth / 2 - width / 2,
                TextAlignment.Right => Layout.PageWidth - Layout.Margin - width,
                _ => Layout.Margin
            };

            _currentPage.AddText(line, x, _currentY, size, dye);
            _currentY += height;
        }
    }

    public void DrawBitmapText(string text, Dye textDye, Dye backgroundDye, int scale)
    {
        var charWidth = TextRenderer.CharWidth * scale;
        var maxCharsPerLine = (int)(Layout.ContentWidth / charWidth);

        var span = text.AsSpan();
        var start = 0;
        while (start < span.Length)
        {
            var index = span[start..].IndexOf('\n');
            var lineLength = index == -1 ? span.Length - start : index;
            var line = span.Slice(start, lineLength).Trim();

            var nextStart = index == -1 ? span.Length : start + index + 1;

            if (!line.IsEmpty)
            {
                var pos = 0;
                while (pos < line.Length)
                {
                    var length = Math.Min(maxCharsPerLine, line.Length - pos);
                    var chunk = line.Slice(pos, length);

                    using var image = TextRenderer.RenderBitmap(chunk, textDye, backgroundDye, scale);
                    DrawImage(image);

                    pos += length;
                }
            }

            start = nextStart;
        }
    }

    internal void DrawImage(BitmapImage image)
    {
        var scale = Math.Min(1f,
            Math.Min(Layout.ContentWidth / image.Width, Layout.ContentHeight / image.Height));
        var width = image.Width * scale;
        var height = image.Height * scale;

        if (_currentY + height > Layout.PageHeight - Layout.Margin + 0.01f)
            StartNewPage();

        var resource = new ImageResource(_nextId++, image.Width, image.Height, image.GetPixelMemory().ToArray());
        _images.Add(resource);

        var x = Layout.PageWidth / 2 - width / 2;
        _currentPage.AddImage(resource, x, _currentY, width, height);
        _currentY += height;
    }

    private void StartNewPage()
    {
        _currentPage = new Page(_nextId++, _background);
        _pages.Add(_currentPage);
        _currentY = Layout.Margin;
    }

    public async Task FinalizeAsync()
    {
        await _buffer.WriteAsync("%PDF-1.7\n%"u8.ToArray());
        await _buffer.WriteAsync("%%Creator: CreatePdf.NET\n"u8.ToArray());
        await _buffer.WriteAsync("\n"u8.ToArray());

        var catalogId = _nextId++;
        var pagesId = _nextId++;
        var fontId = _nextId++;

        await WriteObject(fontId,
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");

        var pageIds = new List<int>();
        foreach (var page in _pages)
        {
            var contentId = _nextId++;
            pageIds.Add(await WritePageAsync(page, pagesId, fontId, contentId));
        }

        foreach (var image in _images)
            await WriteImageAsync(image);

        await WriteObject(pagesId,
            $"<< /Type /Pages /Kids [{string.Join(" ", pageIds.Select(id => $"{id.ToString(CultureInfo.InvariantCulture)} 0 R"))}] /Count {pageIds.Count.ToString(CultureInfo.InvariantCulture)} >>");

        await WriteObject(catalogId,
            $"<< /Type /Catalog /Pages {pagesId.ToString(CultureInfo.InvariantCulture)} 0 R >>");

        await WriteXRefAndTrailerAsync(catalogId);

        _buffer.Position = 0;
        await _buffer.CopyToAsync(_output);
    }

    private async Task<int> WritePageAsync(Page page, int pagesId, int fontId, int contentId)
    {
        var content = page.GetContent();
        var contentBytes = Latin1.GetBytes(content);

        await WriteObject(contentId,
            $"<< /Length {contentBytes.Length} >>",
            "stream",
            contentBytes,
            "endstream");

        var resources = $"<< /Font << /Helvetica {fontId.ToString(CultureInfo.InvariantCulture)} 0 R >>";
        if (page.HasImages)
            resources += $" /XObject << {string.Join(" ", page.ImageIds.Select(id => $"/Im{id.ToString(CultureInfo.InvariantCulture)} {id.ToString(CultureInfo.InvariantCulture)} 0 R"))} >>";
        resources += " >>";

        await WriteObject(page.Id,
            $"""
             << /Type /Page /Parent {pagesId.ToString(CultureInfo.InvariantCulture)} 0 R
                /MediaBox [0 0 {Layout.PageWidth.ToString("F2", CultureInfo.InvariantCulture)} {Layout.PageHeight.ToString("F2", CultureInfo.InvariantCulture)}]
                /Contents {contentId.ToString(CultureInfo.InvariantCulture)} 0 R
                /Resources {resources} >>
             """);

        return page.Id;
    }

    private async Task WriteImageAsync(ImageResource image)
    {
        await WriteObject(image.Id,
            $"""
             << /Type /XObject /Subtype /Image
                /Width {image.Width.ToString(CultureInfo.InvariantCulture)} /Height {image.Height.ToString(CultureInfo.InvariantCulture)}
                /ColorSpace /DeviceRGB /BitsPerComponent 8
                /Length {image.RgbData.Length.ToString(CultureInfo.InvariantCulture)} >>
             """,
            "stream",
            image.RgbData,
            "endstream");
    }

    private async Task WriteObject(int id, string header, string? prefix = null, byte[]? binaryData = null,
        string? suffix = null)
    {
        ref var offset = ref CollectionsMarshal.GetValueRefOrAddDefault(_offsets, id, out _);
        offset = _buffer.Position;

        await WriteLineAsync($"{id.ToString(CultureInfo.InvariantCulture)} 0 obj");
        await WriteLineAsync(header);

        if (prefix != null)
            await WriteLineAsync(prefix);

        if (binaryData != null)
        {
            await _buffer.WriteAsync(binaryData);
            await _buffer.WriteAsync("\n"u8.ToArray());
        }

        if (suffix != null)
            await WriteLineAsync(suffix);

        await WriteLineAsync("endobj");
    }

    private async Task WriteXRefAndTrailerAsync(int rootId)
    {
        var xrefPos = _buffer.Position;

        await WriteLineAsync("xref");
        await WriteLineAsync($"0 {(_offsets.Count + 1).ToString(CultureInfo.InvariantCulture)}");
        await WriteLineAsync("0000000000 65535 f ");

        foreach (var (_, offset) in _offsets.OrderBy(x => x.Key))
            await WriteLineAsync($"{offset.ToString("D10", CultureInfo.InvariantCulture)} 00000 n ");

        await WriteLineAsync($"trailer << /Size {(_offsets.Count + 1).ToString(CultureInfo.InvariantCulture)} /Root {rootId.ToString(CultureInfo.InvariantCulture)} 0 R >>");
        await WriteLineAsync("startxref");
        await WriteLineAsync(xrefPos.ToString(CultureInfo.InvariantCulture));
        await _buffer.WriteAsync("%%EOF"u8.ToArray());
    }

    private async Task WriteLineAsync(string text)
    {
        var bytes = Encoding.ASCII.GetBytes(text);
        await _buffer.WriteAsync(bytes);
        await _buffer.WriteAsync("\n"u8.ToArray());
    }
}
