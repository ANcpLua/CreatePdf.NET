FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

WORKDIR /app

COPY . .
RUN dotnet build CreatePdf.NET/CreatePdf.NET.csproj -c Release

RUN dotnet new console -n Demo -o /demo
WORKDIR /demo

RUN dotnet add reference /app/CreatePdf.NET/CreatePdf.NET.csproj

RUN echo 'using CreatePdf.NET.Public;\n\
\n\
await Pdf.Create()\n\
    .AddText("Hello from Docker!", Dye.Blue, TextSize.Large)\n\
    .AddLine()\n\
    .AddText("CreatePdf.NET Demo")\n\
    .AddLine()\n\
    .AddPixelText("Running in a container!", Dye.Green)\n\
    .SaveAsync("docker-demo");\n\
\n\
Console.WriteLine("PDF created successfully!");' > Program.cs

ENTRYPOINT ["dotnet", "run"]