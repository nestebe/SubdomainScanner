using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SubdomainScanner.Core;
using SubdomainScanner.Sources;
using SubdomainScanner.Utils;

namespace SubdomainScanner
{
    /// <summary>
    /// SubdomainScanner - Subdomain Discovery Tool
    /// </summary>
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            PrintBanner();

            if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
            {
                PrintUsage();
                return 0;
            }

            string? domain = null;
            string? outputFile = null;
            string? format = "txt";
            bool resolve = false;
            bool verbose = false;
            var disabledSources = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-d":
                    case "--domain":
                        if (i + 1 < args.Length)
                        {
                            domain = args[++i];
                        }
                        break;
                    case "-o":
                    case "--output":
                        if (i + 1 < args.Length)
                        {
                            outputFile = args[++i];
                        }
                        break;
                    case "-f":
                    case "--format":
                        if (i + 1 < args.Length)
                        {
                            format = args[++i].ToLower();
                        }
                        break;
                    case "-r":
                    case "--resolve":
                        resolve = true;
                        break;
                    case "-v":
                    case "--verbose":
                        verbose = true;
                        break;
                    case "--disable":
                        if (i + 1 < args.Length)
                        {
                            disabledSources.Add(args[++i].ToLower());
                        }
                        break;
                    case "-l":
                    case "--list-sources":
                        ListSources();
                        return 0;
                }
            }

            if (string.IsNullOrWhiteSpace(domain))
            {
                Console.WriteLine("[!] Error: Domain is required.");
                PrintUsage();
                return 1;
            }

            try
            {
                using (var scanner = new Core.SubdomainScanner(domain))
                {
                    var httpClient = scanner.GetHttpClient();

                    // Initialize all sources
                    var sources = new List<ISubdomainSource>
                    {
                        new CrtShSource(httpClient),
                        new HackerTargetSource(httpClient),
                        new WaybackMachineSource(httpClient),
                        new AlienVaultSource(httpClient),
                        new ThreatCrowdSource(httpClient),
                        new CommonCrawlSource(httpClient)
                    };

                    // Disable requested sources
                    foreach (var source in sources)
                    {
                        if (disabledSources.Contains(source.Name.ToLower()))
                        {
                            source.IsEnabled = false;
                            Console.WriteLine($"[!] Source disabled: {source.Name}");
                        }
                        scanner.AddSource(source);
                    }

                    // Launch scan
                    var subdomains = await scanner.ScanAsync();

                    Console.WriteLine();

                    if (subdomains.Count > 0)
                    {
                        if (!resolve)
                        {
                            Console.WriteLine("[*] List of subdomains:");
                            foreach (var subdomain in subdomains)
                            {
                                Console.WriteLine($"    {subdomain}");
                            }
                        }
                        else
                        {
                            var resolved = await scanner.ResolveAsync(subdomains);
                            Console.WriteLine();
                            Console.WriteLine($"[*] Resolved subdomains: {resolved.Count}/{subdomains.Count}");

                            // Export CSV if resolution
                            if (!string.IsNullOrWhiteSpace(outputFile) && format == "csv")
                            {
                                FileExporter.ExportToCsv(resolved, outputFile);
                                return 0;
                            }
                        }

                        // Export results
                        if (!string.IsNullOrWhiteSpace(outputFile))
                        {
                            Console.WriteLine();
                            switch (format)
                            {
                                case "json":
                                    FileExporter.ExportToJson(subdomains, outputFile);
                                    break;
                                case "txt":
                                default:
                                    FileExporter.ExportToText(subdomains, outputFile);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("[!] No subdomains found.");
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error: {ex.Message}");
                if (verbose)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return 1;
            }
        }

        static void PrintBanner()
        {
            Console.WriteLine();
            Console.WriteLine("╔═══════════════════════════════════════════════╗");
            Console.WriteLine("║      SubdomainScanner v2.0 - C# Edition      ║");
            Console.WriteLine("║       Subdomain Discovery Tool               ║");
            Console.WriteLine("║   Author: Nicolas ESTEBE / devestebe@gmail.com");
            Console.WriteLine("╚═══════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  SubdomainScanner -d <domain> [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -d, --domain <domain>    Target domain to scan (required)");
            Console.WriteLine("  -o, --output <file>      Save results to a file");
            Console.WriteLine("  -f, --format <format>    Export format: txt, json, csv (default: txt)");
            Console.WriteLine("  -r, --resolve            Resolve IP addresses of subdomains");
            Console.WriteLine("  -v, --verbose            Verbose mode with error details");
            Console.WriteLine("  --disable <source>       Disable a specific source");
            Console.WriteLine("  -l, --list-sources       List all available sources");
            Console.WriteLine("  -h, --help               Display this help");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  SubdomainScanner -d example.com");
            Console.WriteLine("  SubdomainScanner -d example.com -r -o results.csv -f csv");
            Console.WriteLine("  SubdomainScanner -d example.com -o results.json -f json");
            Console.WriteLine("  SubdomainScanner -d example.com --disable wayback");
            Console.WriteLine();
        }

        static void ListSources()
        {
            Console.WriteLine("Available sources:");
            Console.WriteLine("  - crt.sh           : Certificate Transparency Logs");
            Console.WriteLine("  - hackertarget     : HackerTarget API");
            Console.WriteLine("  - wayback          : Wayback Machine (Internet Archive)");
            Console.WriteLine("  - alienvault       : AlienVault OTX");
            Console.WriteLine("  - threatcrowd      : ThreatCrowd API");
            Console.WriteLine("  - commoncrawl      : CommonCrawl Index");
            Console.WriteLine();
        }
    }
}
