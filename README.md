[![codecov](https://codecov.io/gh/ANcpLua/CreatePdf.NET/branch/main/graph/badge.svg?token=lgxIXBnFrn)](https://codecov.io/gh/ANcpLua/CreatePdf.NET)
[![.NET](https://img.shields.io/badge/.NET-10.0_Preview-512BD4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![NuGet](https://img.shields.io/nuget/v/CreatePdf.NET?label=NuGet&color=0891B2)](https://www.nuget.org/packages/CreatePdf.NET/)
[![License](https://img.shields.io/github/license/ANcpLua/CreatePdf.NET?label=License&color=white)](https://github.com/ANcpLua/CreatePdf.NET/blob/main/LICENSE)
[![Docker](https://img.shields.io/docker/v/ancplua/createpdf.net?label=Docker&color=0C4A6E)](https://hub.docker.com/r/ancplua/createpdf.net)
# CreatePdf.NET

A simple, .NET library for PDF creation with text and bitmap rendering,
plus [optional OCR functionality](#to-enable-ocr-functionality) for text extraction.


## Usage

```cs
using CreatePdf.NET;

await Pdf.Create(Dye.Black)
    .AddText("Hello World")
    .SaveAsync("text.pdf");

await Pdf.Create()
    .AddPixelText("Hello World")
    .SaveAsync("pixel.pdf");

await Pdf.Create()
    .AddText("Hello World!")
    .SaveAndOcr("text-ocr-demo");

await Pdf.Create()
    .AddPixelText("Hello World!")
    .SaveAndOcr("pixel-ocr-demo");

await Pdf.Create()
    .AddText("Hello World!", Dye.Blue, TextSize.Large)
    .AddLine()
    .SaveAndOpenAsync("opens-the-pdf.pdf");

await Pdf.Create(Dye.White)
    .AddPixelText("Hello World!", Dye.Red, Dye.Brown, PixelTextSize.Medium)
    .AddLines(5)
    .SaveAndShowDirectoryAsync("opens-the-directory.pdf");
```

### Stream-based PDF Processing

```cs
// Basic OCR with default settings
await Pdf.Load(pdfStream).OcrAsync();

// Custom OCR settings for individual needs
await Pdf.Load(pdfStream).OcrAsync(new OcrOptions
{
    Dpi = 600,          
    PageSegmentationMode = 3
});
```

### To enable OCR functionality

| Platform    | Installation                                                                                                                            |
|-------------|-----------------------------------------------------------------------------------------------------------------------------------------|
| **macOS**   | `brew install tesseract`                                                                                                                |
| **Windows** | Download [Tesseract](https://github.com/UB-Mannheim/tesseract/wiki) and [Ghostscript](https://www.ghostscript.com/download/gsdnld.html) |
| **Linux**   | `sudo apt-get install ghostscript tesseract-ocr`                                                                                        |

> **Note**: Only needed for OCR. 
> 
> PDF generation works without it.
>

## Installation

```bash
dotnet add package CreatePdf.NET --prerelease
```

## Requirements

- .NET 10.0 (Preview)

## License

This project is licensed under the [MIT License](LICENSE).

