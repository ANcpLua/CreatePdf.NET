[![codecov](https://codecov.io/gh/ANcpLua/CreatePdf.NET/branch/main/graph/badge.svg?token=lgxIXBnFrn)](https://codecov.io/gh/ANcpLua/CreatePdf.NET)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-7C3AED)](https://dotnet.microsoft.com/download/dotnet/10.0)[![.NET 9](https://img.shields.io/badge/-9.0-6366F1)](https://dotnet.microsoft.com/download/dotnet/9.0)[![.NET 8](https://img.shields.io/badge/-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![NuGet](https://img.shields.io/nuget/v/CreatePdf.NET?label=NuGet&color=0891B2)](https://www.nuget.org/packages/CreatePdf.NET/)
[![License](https://img.shields.io/github/license/ANcpLua/CreatePdf.NET?label=License&color=white)](https://github.com/ANcpLua/CreatePdf.NET/blob/main/LICENSE)
[![Docker](https://img.shields.io/docker/v/ancplua/createpdf.net?label=Docker&color=0C4A6E)](https://hub.docker.com/r/ancplua/createpdf.net)

# CreatePdf.NET

A simple, .NET library for PDF creation with text and bitmap rendering.

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
    .AddText("Hello World!", color: Dye.Blue, size: TextSize.Large)
    .AddLine()
    .SaveAndOpenAsync("opens-the-pdf.pdf");

await Pdf.Create(Dye.White)
    .AddPixelText("Hello World!", textColor: Dye.Red, backgroundColor: Dye.Brown, size: PixelTextSize.Medium)
    .AddLines(5)
    .SaveAndShowDirectoryAsync("opens-the-directory.pdf");
```

## Installation

```bash
dotnet add package CreatePdf.NET
```

### Requirements

- .NET 8.0, 9.0, or 10.0 SDK

## License

This project is licensed under the [MIT License](LICENSE).
