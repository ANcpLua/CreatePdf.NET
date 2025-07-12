using System.Diagnostics.CodeAnalysis;

namespace CreatePdf.NET.Internal;

internal interface IContent
{
    void Render(IPdfCanvas canvas);
}

internal interface IPdfCanvas
{
    void DrawText(string text, int size, Dye dye, TextAlignment alignment);
    void DrawBitmapText(string text, Dye textDye, Dye backgroundDye, int scale);
}

[ExcludeFromCodeCoverage]
internal record TextContent(string Text, int Size, Dye Dye, TextAlignment Alignment) : IContent
{
    public void Render(IPdfCanvas canvas)
    {
        canvas.DrawText(Text, Size, Dye, Alignment);
    }
}

[ExcludeFromCodeCoverage]
internal record BitmapTextContent(string Text, Dye TextDye, Dye BackgroundDye, int Scale) : IContent
{
    public void Render(IPdfCanvas canvas)
    {
        canvas.DrawBitmapText(Text, TextDye, BackgroundDye, Scale);
    }
}

[ExcludeFromCodeCoverage]
internal record ImageResource(int Id, int Width, int Height, byte[] RgbData);

internal static class Layout
{
    public const float PageWidth = 595f;
    public const float PageHeight = 842f;
    public const float Margin = 50f;
    public const float ContentWidth = PageWidth - 2 * Margin;
    public const float ContentHeight = PageHeight - 2 * Margin;
    public const float LineSpacing = 1.5f;
}