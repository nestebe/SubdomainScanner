# SubdomainScanner v2.0

Subdomain discovery tool in C# .NET, similar to Sublist3r, with modular architecture and multiple sources.

**Author:** Nicolas ESTEBE / devestebe@gmail.com

## Features

- **Modular architecture** - Easily extensible data sources
- **6 public sources** - crt.sh, HackerTarget, Wayback Machine, AlienVault OTX, ThreatCrowd, CommonCrawl
- **DNS resolution** - Verification of active subdomains with IP addresses
- **Multi-format export** - TXT, JSON, CSV
- **Flexible configuration** - Enable/disable individual sources
- **Asynchronous** - Parallel search across all sources
- **Intuitive CLI interface** - With complete help

## Project Architecture

```
SubdomainScanner/
├── Core/                      # Core components
│   ├── ISubdomainSource.cs    # Interface for sources
│   ├── BaseSubdomainSource.cs # Abstract base class
│   └── SubdomainScanner.cs    # Main manager
├── Sources/                   # Source implementations
│   ├── CrtShSource.cs
│   ├── HackerTargetSource.cs
│   ├── WaybackMachineSource.cs
│   ├── AlienVaultSource.cs
│   ├── ThreatCrowdSource.cs
│   └── CommonCrawlSource.cs
├── Utils/                     # Utilities
│   └── FileExporter.cs        # File export
└── Program.cs                 # CLI entry point
```

## Data Sources

| Source | Description | Type |
|--------|-------------|------|
| **crt.sh** | Certificate Transparency Logs | SSL/TLS Certificates |
| **HackerTarget** | Public search API | DNS & Web |
| **Wayback Machine** | Internet Archive | Web history |
| **AlienVault OTX** | Open Threat Exchange | Threat Intelligence |
| **ThreatCrowd** | Domain search | Security |
| **CommonCrawl** | Web crawl index | Web crawling |

## Installation

```bash
cd SubdomainScanner
dotnet build -c Release
```

## Usage

### Basic Examples

```bash
# Scan a domain with all sources
dotnet run -- -d example.com

# List available sources
dotnet run -- -l

# Scan with DNS resolution
dotnet run -- -d example.com -r

# Export to JSON
dotnet run -- -d example.com -o results.json -f json

# Export to CSV with DNS resolution
dotnet run -- -d example.com -r -o results.csv -f csv

# Disable specific sources
dotnet run -- -d example.com --disable wayback --disable commoncrawl

# Verbose mode
dotnet run -- -d example.com -v
```

### Complete Options

```
Options:
  -d, --domain <domain>    Target domain to scan (required)
  -o, --output <file>      Save results to a file
  -f, --format <format>    Export format: txt, json, csv (default: txt)
  -r, --resolve            Resolve IP addresses of subdomains
  -v, --verbose            Verbose mode with error details
  --disable <source>       Disable a specific source
  -l, --list-sources       List all available sources
  -h, --help               Display help
```

## Compile to Executable

### Windows
```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

### Linux
```bash
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
```

### macOS
```bash
dotnet publish -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true
```

The executable will be in `bin/Release/net9.0/<runtime>/publish/`

## Output Examples

### TXT Format
```
api.github.com
www.github.com
gist.github.com
```

### JSON Format
```json
{
  "timestamp": "2025-01-15T10:30:00Z",
  "total": 296,
  "subdomains": [
    "api.github.com",
    "www.github.com",
    "gist.github.com"
  ]
}
```

### CSV Format (with -r)
```csv
Subdomain,IP Address
api.github.com,140.82.121.6
www.github.com,140.82.121.4
```

## Add a New Source

1. Create a class in `Sources/` inheriting from `BaseSubdomainSource`
2. Implement `SearchAsync(string domain)`
3. Add the source in `Program.cs`

Example:
```csharp
public class MySource : BaseSubdomainSource
{
    public override string Name => "MySource";

    public MySource(HttpClient httpClient) : base(httpClient) { }

    public override async Task<HashSet<string>> SearchAsync(string domain)
    {
        LogInfo("Searching...");
        // Your logic here
        var results = new HashSet<string>();
        return CleanSubdomains(results, domain);
    }
}
```

## Performance

- Asynchronous parallel search
- Configurable timeout (30s default)
- Automatic result deduplication
- Per-source error handling (one error doesn't stop others)

## Ethical Use

This tool is intended for defensive security purposes only:
- ✅ Authorized penetration testing
- ✅ Security audit of your own domains
- ✅ Cybersecurity research
- ❌ Unauthorized scanning
- ❌ Malicious reconnaissance

**Never use this tool against domains without explicit authorization.**

## License

Educational project - Responsible use only

## Credits

Inspired by Sublist3r - Rewritten in C# with modular architecture
