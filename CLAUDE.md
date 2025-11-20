# Project Guidelines for Claude Code

## Project Information

- **Repository**: ANcpLua/CreatePdf.NET
- **Package ID**: CreatePdf.NET
- **Current Version**: Check `CreatePdf.NET/CreatePdf.NET.csproj`
- **Project Path**: `/Users/ancplua/CreatePdf.NET`
- **Main Branch**: main
- **Multi-Targeting**: .NET 10.0, 9.0, 8.0

## Versioning & Release Strategy

### Semantic Versioning (MAJOR.MINOR.PATCH)

- **MAJOR** (x.0.0) - Breaking API changes, major architectural changes
- **MINOR** (3.x.0) - New features, non-breaking additions
- **PATCH** (3.0.x) - Bug fixes, minor tweaks, icon updates, documentation

### Release Rules

#### Major Versions (x.0.0)
- ✅ Bump version in `.csproj`
- ✅ Commit and push to GitHub
- ✅ Create GitHub Release with detailed release notes
- ✅ Publish to NuGet
- 📝 Document breaking changes and migration guide

#### Minor Versions (3.x.0)
- ✅ Bump version in `.csproj`
- ✅ Commit and push to GitHub
- ✅ Create GitHub Release with feature descriptions
- ✅ Publish to NuGet
- 📝 Document new features and usage examples

#### Patch Versions (3.0.x)
- ✅ Bump version in `.csproj`
- ✅ Commit and push to GitHub
- ❌ NO GitHub Release needed
- ✅ Publish to NuGet
- 💡 Optional: Create git tag for tracking
- 💡 Keep commit messages clear (they're the documentation)

### Quick Reference

| Change Type | Example | GitHub Release? | NuGet Publish? |
|-------------|---------|-----------------|----------------|
| Breaking change | API removal | ✅ YES | ✅ YES |
| New feature | Add bitmap support | ✅ YES | ✅ YES |
| Bug fix | Fix text rendering | ❌ NO | ✅ YES |
| Icon update | Adjust positioning | ❌ NO | ✅ YES |
| Documentation | Update README | ❌ NO | ❌ NO* |

*Documentation-only changes don't need version bumps

## Current Automation

### GitHub Actions Workflows

#### `nuget-publish.yml`
- Triggers:
  - GitHub Release published
  - Manual workflow dispatch
- Runs:
  1. Auto-center/optimize icon (ImageMagick: trim, resize 512x512, transparent background)
  2. Build, test, pack, publish to NuGet
- Authentication: NuGet Trusted Publishing (OIDC) - no API key needed

### Publishing Workflows

#### Option 1: Automated Publishing (Recommended for Major/Minor)

**For major and minor versions**, create a GitHub Release:

1. Go to https://github.com/ANcpLua/CreatePdf.NET/releases/new
2. Create new tag: `v3.x.0`
3. Generate release notes
4. Publish release
5. GitHub Actions will automatically build, test, optimize icon, and publish to NuGet via Trusted Publishing

#### Option 2: Manual Workflow Dispatch

Trigger the workflow manually from GitHub Actions UI for any version without creating a release.

#### Option 3: Manual Publishing (Current for Patches)

For patch versions, we manually publish:

```bash
# Working directory: /Users/ancplua/CreatePdf.NET

# 1. Bump version in .csproj
# Edit: CreatePdf.NET/CreatePdf.NET.csproj
# Change: <Version>3.0.x</Version>

# 2. Commit and push
git add CreatePdf.NET/CreatePdf.NET.csproj icon.png
git commit -m "v3.0.x - Description"
git push

# 3. Build package
dotnet pack CreatePdf.NET/CreatePdf.NET.csproj --configuration Release

# 4. Publish to NuGet (using Trusted Publishing or API key)
# With Trusted Publishing (if configured):
# - Use GitHub Actions workflow dispatch
# OR with API key:
dotnet nuget push CreatePdf.NET/bin/Release/CreatePdf.NET.3.0.x.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json \
  --skip-duplicate
```

**Note**: Trusted Publishing is configured, so API keys are optional. Prefer using GitHub Actions workflows.

## Package Metadata

### Icon Requirements
- Size: 512x512 PNG (scales down to 128x128, 64x64)
- Location: `icon.png` in repository root
- Referenced in `.csproj`: `<PackageIcon>icon.png</PackageIcon>`
- **Auto-optimization**: GitHub Actions workflow automatically trims, resizes, and centers icon before packing
- Note: NuGet may take 5-10 minutes to update icon after publish

### Tags for Discoverability
Current tags ensure package appears in searches for:
- PDF creation, generation, document generation
- .NET 10, .NET 9, .NET 8
- Text rendering, bitmap graphics, OCR
- Reports, invoices, receipts, labels

## Testing Requirements

All tests must pass before publishing:
- Unit tests: Core PDF generation, text rendering, bitmap handling
- Integration tests: OCR functionality with Tesseract
- 100% code coverage target
- Tests run on .NET 10, 9, and 8

### Multi-Targeting Notes
- Project targets net10.0, net9.0, net8.0
- Tests ensure compatibility across all frameworks
- Use conditional compilation when needed (`#if NET10_0_OR_GREATER`)

## Commit Message Guidelines

Keep commit messages clear and concise:

```bash
# Good examples (patches)
"Fix text alignment in pixel rendering"
"v3.0.3 - Adjust icon positioning for better NuGet display"

# Good examples (features/majors)
"v3.0.0 - Add multi-framework support (.NET 8/9/10)"
"v2.5.0 - Add OCR text extraction with Tesseract"

# Avoid
"Update code" ❌
"Fix stuff" ❌
"Various improvements" ❌
```

## Architecture Notes

### PDF Generation
- **Core API**: Fluent interface for building PDFs (`Pdf.Create()`)
- **Text Rendering**: Two modes - native PDF text and pixel-based rendering
- **Bitmap Support**: PNG/JPG image embedding
- **File I/O**: All operations are async-first

### OCR Integration
- Optional Tesseract integration for text extraction
- External dependency (tesseract binary)
- Abstracted via `IOcrProvider` interface for testability

### Multi-Framework Support
- Targets .NET 10.0, 9.0, 8.0 for maximum compatibility
- Uses C# 14 language features (LangVersion: 14)
- Nullable reference types enabled

## .NET 10 Best Practices

### Key Language Features (C# 14)

**Field-Backed Properties:**
```csharp
public string Message
{
    get;
    set => field = value ?? throw new ArgumentNullException(nameof(value));
}
```

**Implicit Usings:**
- Project uses `<ImplicitUsings>enable</ImplicitUsings>`
- Common namespaces automatically imported

**Nullable Reference Types:**
- Enabled project-wide: `<Nullable>enable</Nullable>`
- Helps prevent null reference exceptions

### Library-Specific Recommendations

#### SourceLink Configuration (Critical for Debugging)
Consider adding to `.csproj` for better consumer debugging:
```xml
<PropertyGroup>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.SourceLink.GitHub" Version="*" PrivateAssets="All"/>
</ItemGroup>
```

#### Documentation Generation
- Already enabled: `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
- XML comments are included in NuGet package

#### Symbol Packages
- Already configured: `<SymbolPackageFormat>snupkg</SymbolPackageFormat>`
- Debugging symbols published separately

### Testing Best Practices

**100% Code Coverage:**
- Project aims for comprehensive test coverage
- Use Codecov for tracking coverage metrics
- All public APIs should have tests

**Multi-Framework Testing:**
- Tests run on all target frameworks
- Ensures compatibility across .NET versions

**External Dependencies:**
- OCR tests require Tesseract binary
- Use abstractions for testability
- Mock external dependencies in unit tests

## Official API Documentation Links

**Microsoft Learn (Always Current):**
- [.NET 10 Release Notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview)
- [C# 14 What's New](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [Multi-Targeting in SDK-Style Projects](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet Package Authoring Best Practices](https://learn.microsoft.com/en-us/nuget/create-packages/package-authoring-best-practices)
- [SourceLink Configuration](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink)
- [Nullable Reference Types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)

**Last Updated:** November 20, 2025
