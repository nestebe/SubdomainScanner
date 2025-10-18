using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SubdomainScanner
{
    public class SubdomainFinder : IDisposable
    {
        private readonly string _domain;
        private readonly HashSet<string> _foundSubdomains;
        private readonly HttpClient _httpClient;

        public SubdomainFinder(string domain)
        {
            _domain = domain.ToLower().Trim();
            _foundSubdomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) SubdomainScanner/1.0");
        }

        public async Task<List<string>> FindSubdomains()
        {
            Console.WriteLine($"[*] Recherche de sous-domaines pour: {_domain}");
            Console.WriteLine();

            var tasks = new List<Task>
            {
                SearchCertificateTransparency(),
                SearchHackerTarget(),
                SearchDNSDumpster(),
                SearchCrtSh(),
                SearchVirusTotal()
            };

            await Task.WhenAll(tasks);

            var sortedSubdomains = _foundSubdomains.OrderBy(s => s).ToList();
            return sortedSubdomains;
        }

        private async Task SearchCertificateTransparency()
        {
            Console.WriteLine("[+] Recherche via crt.sh...");
            try
            {
                var url = $"https://crt.sh/?q=%.{_domain}&output=json";
                var response = await _httpClient.GetStringAsync(url);

                var matches = Regex.Matches(response, @"""name_value"":""([^""]+)""");
                foreach (Match match in matches)
                {
                    var subdomain = match.Groups[1].Value.ToLower().Trim();
                    if (subdomain.EndsWith(_domain) && !subdomain.StartsWith("*"))
                    {
                        AddSubdomain(subdomain);
                    }
                }
                Console.WriteLine($"    Trouvé {_foundSubdomains.Count} sous-domaines via crt.sh");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Erreur crt.sh: {ex.Message}");
            }
        }

        private async Task SearchCrtSh()
        {
            // Recherche alternative avec crt.sh
            Console.WriteLine("[+] Recherche alternative via crt.sh...");
            try
            {
                var url = $"https://crt.sh/?q={_domain}&output=json";
                var response = await _httpClient.GetStringAsync(url);

                var matches = Regex.Matches(response, @"""common_name"":""([^""]+)""");
                foreach (Match match in matches)
                {
                    var subdomain = match.Groups[1].Value.ToLower().Trim();
                    if (subdomain.EndsWith(_domain) && !subdomain.StartsWith("*"))
                    {
                        AddSubdomain(subdomain);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Erreur recherche alternative: {ex.Message}");
            }
        }

        private async Task SearchHackerTarget()
        {
            Console.WriteLine("[+] Recherche via HackerTarget...");
            try
            {
                var url = $"https://api.hackertarget.com/hostsearch/?q={_domain}";
                var response = await _httpClient.GetStringAsync(url);

                var lines = response.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains(","))
                    {
                        var subdomain = line.Split(',')[0].Trim().ToLower();
                        if (subdomain.EndsWith(_domain))
                        {
                            AddSubdomain(subdomain);
                        }
                    }
                }
                Console.WriteLine($"    Trouvé {_foundSubdomains.Count} sous-domaines via HackerTarget");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Erreur HackerTarget: {ex.Message}");
            }
        }

        private async Task SearchDNSDumpster()
        {
            Console.WriteLine("[+] Recherche via DNSDumpster API alternative...");
            try
            {
                // Utilisation d'une API alternative car DNSDumpster nécessite CSRF token
                var url = $"https://api.securitytrails.com/v1/domain/{_domain}/subdomains";
                // Note: Cette API nécessite une clé API, donc on skip pour l'instant
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Erreur DNSDumpster: {ex.Message}");
            }
        }

        private async Task SearchVirusTotal()
        {
            Console.WriteLine("[+] Recherche via VirusTotal (publique)...");
            try
            {
                // Note: L'API VirusTotal nécessite une clé API
                // On peut tenter via le site public mais c'est limité
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Erreur VirusTotal: {ex.Message}");
            }
        }

        private void AddSubdomain(string subdomain)
        {
            subdomain = subdomain.ToLower().Trim();

            // Nettoyer les entrées avec multiples sous-domaines séparés par des espaces ou newlines
            // Remplacer les échappements littéraux \n, \r, \t par leurs équivalents
            subdomain = subdomain.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");

            var parts = subdomain.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var cleanedSubdomain = part.Trim();

                // Ignorer les entrées invalides (emails, wildcards, etc.)
                if (!string.IsNullOrWhiteSpace(cleanedSubdomain) &&
                    cleanedSubdomain.EndsWith(_domain) &&
                    !cleanedSubdomain.StartsWith("*") &&
                    !cleanedSubdomain.Contains("@") &&
                    cleanedSubdomain.Contains("."))
                {
                    _foundSubdomains.Add(cleanedSubdomain);
                }
            }
        }

        public async Task<Dictionary<string, string>> ResolveSubdomains(List<string> subdomains)
        {
            Console.WriteLine();
            Console.WriteLine("[*] Résolution DNS des sous-domaines...");

            var results = new Dictionary<string, string>();
            var tasks = subdomains.Select(async subdomain =>
            {
                try
                {
                    var addresses = await Dns.GetHostAddressesAsync(subdomain);
                    if (addresses.Length > 0)
                    {
                        var ip = addresses[0].ToString();
                        lock (results)
                        {
                            results[subdomain] = ip;
                        }
                        Console.WriteLine($"    {subdomain} -> {ip}");
                    }
                }
                catch
                {
                    // Ignore les erreurs de résolution DNS
                }
            });

            await Task.WhenAll(tasks);
            return results;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
