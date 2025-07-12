using System.Buffers;

namespace CreatePdf.NET.Internal;

internal sealed class BitmapImage : IDisposable
{
    private readonly IMemoryOwner<byte> _memory;

    public BitmapImage(int width, int height, IMemoryOwner<byte> memory)
    {
        Width = width;
        Height = height;
        Size = width * height * 3;
        _memory = memory;
    }

    public int Width { get; }
    public int Height { get; }
    public int Size { get; }

    public void Dispose()
    {
        _memory.Dispose();
    }

    public static BitmapImage FromBitmap(bool[] bitmap, int width, int height, Dye fg, Dye bg)
    {
        var size = width * height * 3;
        var memory = MemoryPool<byte>.Shared.Rent(size);
        var pixels = memory.Memory.Span[..size];

        if (fg.IsSimilarTo(bg))
            fg = bg.IsLight ? Dye.Black : Dye.White;

        var bgBytes = new[] { 
            (byte)Math.Round(bg.R * 255), 
            (byte)Math.Round(bg.G * 255), 
            (byte)Math.Round(bg.B * 255) 
        };
        var fgBytes = new[] { 
            (byte)Math.Round(fg.R * 255), 
            (byte)Math.Round(fg.G * 255), 
            (byte)Math.Round(fg.B * 255) 
        };

        for (var i = 0; i < size; i += 3)
            bgBytes.CopyTo(pixels[i..]);

        for (var i = 0; i < bitmap.Length && i * 3 < size; i++)
            if (bitmap[i])
                fgBytes.CopyTo(pixels[(i * 3)..]);

        return new BitmapImage(width, height, memory);
    }

    public ReadOnlyMemory<byte> GetPixelMemory()
    {
        return _memory.Memory[..Size];
    }
}