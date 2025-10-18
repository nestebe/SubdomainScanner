using System.Collections.Generic;
using System.Threading.Tasks;

namespace SubdomainScanner.Core
{
    /// <summary>
    /// Interface for subdomain search sources
    /// </summary>
    public interface ISubdomainSource
    {
        /// <summary>
        /// Source name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Indicates whether the source is enabled
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Searches for subdomains for a given domain
        /// </summary>
        /// <param name="domain">The domain to analyze</param>
        /// <returns>List of found subdomains</returns>
        Task<HashSet<string>> SearchAsync(string domain);
    }
}
