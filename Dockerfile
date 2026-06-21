FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:548d93f8a18a1acbe6cc127bc4f47281430d34a9e35c18afa80a8d6741c2adc3 AS build

WORKDIR /app

COPY . .
RUN dotnet build CreatePdf.NET/CreatePdf.NET.csproj -c Release

RUN dotnet new console -n Demo -o /demo
WORKDIR /demo

RUN dotnet add reference /app/CreatePdf.NET/CreatePdf.NET.csproj

RUN echo 'using CreatePdf.NET.Public;\n\
\n\
await Pdf.Create()\n\
    .AddText("Hello from Docker!", color: Dye.Blue, size: TextSize.Large)\n\
    .AddLine()\n\
    .AddText("CreatePdf.NET Demo")\n\
    .AddLine()\n\
    .AddPixelText("Running in a container!", Dye.Green)\n\
    .SaveAsync("docker-demo");\n\
\n\
Console.WriteLine("PDF created successfully!");' > Program.cs

RUN groupadd -r appuser && useradd -r -g appuser appuser

RUN chown -R appuser:appuser /demo

USER appuser

ENTRYPOINT ["dotnet", "run"]