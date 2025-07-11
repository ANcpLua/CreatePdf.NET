# CreatePdf.NET

[![codecov](https://codecov.io/gh/ANcpLua/CreatePdf.NET/branch/master/graph/badge.svg)](https://codecov.io/gh/ANcpLua/CreatePdf.NET)
[![.NET](https://img.shields.io/badge/.NET-10.0_Preview-512BD4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![NuGet](https://img.shields.io/nuget/v/CreatePdf.NET?label=NuGet&color=blue)](https://www.nuget.org/packages/CreatePdf.NET/)
[![License](https://img.shields.io/github/license/ANcpLua/CreatePdf.NET?label=License&color=blue)](https://github.com/ANcpLua/CreatePdf.NET/blob/master/LICENSE)

A simple, .NET library for PDF creation with text and bitmap rendering, plus [optional OCR functionality](#to-enable-ocr-functionality) for text extraction.

## Installation

```bash
dotnet add package CreatePdf.NET --prerelease
```

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

### To enable OCR functionality

| Platform | Installation |
|----------|-------------|
| **macOS** | `brew install tesseract` |
| **Windows** | Download [Tesseract](https://github.com/UB-Mannheim/tesseract/wiki) and [Ghostscript](https://www.ghostscript.com/download/gsdnld.html) |
| **Linux** | `sudo apt-get install ghostscript tesseract-ocr` |

> **Note**: Only needed if using the `SaveAndOcr` extension method. PDF generation works without these.

## Requirements

- .NET 10.0 (Preview)

## License

This project is licensed under the [MIT License](LICENSE).