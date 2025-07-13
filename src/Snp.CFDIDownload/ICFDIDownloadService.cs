using System.Threading.Tasks;

namespace Snp.CFDIDownload
{
    /// <summary>
    /// Defines operations for interacting with the SAT CFDI mass download service.
    /// </summary>
    public interface ICFDIDownloadService
    {
        /// <summary>
        /// Authenticates with SAT and returns an authorization token.
        /// </summary>
        Task<string> AuthenticateAsync(string rfc, string certificatePath, string privateKeyPath, string privateKeyPassword);

        /// <summary>
        /// Downloads CFDIs matching the specified query and saves them to the provided folder.
        /// </summary>
        Task DownloadAsync(string authToken, string outputFolder, string rfc, string startDate, string endDate);
    }
}
