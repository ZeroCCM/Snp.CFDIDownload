using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Snp.CFDIDownload
{
    /// <summary>
    /// Simplified implementation of the SAT CFDI mass download service.
    /// This code illustrates how to perform the authentication and
    /// download requests using the official SOAP endpoints.
    /// It does not contain production ready error handling.
    /// </summary>
    public class CFDIDownloadService : ICFDIDownloadService
    {
        private const string AuthUrl = "https://certificador.sat.gob.mx/usuariosv3";
        private const string DownloadUrl = "https://descargamasiva.sat.gob.mx/CFDIService";
        private readonly HttpClient _httpClient;

        public CFDIDownloadService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> AuthenticateAsync(string rfc, string certificatePath, string privateKeyPath, string privateKeyPassword)
        {
            if (string.IsNullOrWhiteSpace(rfc))
                throw new ArgumentException("RFC is required", nameof(rfc));
            if (!File.Exists(certificatePath))
                throw new FileNotFoundException("Certificate file not found", certificatePath);
            if (!File.Exists(privateKeyPath))
                throw new FileNotFoundException("Private key file not found", privateKeyPath);

            var cert = new X509Certificate2(certificatePath, privateKeyPassword, X509KeyStorageFlags.MachineKeySet);
            var soap = BuildAuthEnvelope(cert, rfc);
            var content = new StringContent(soap, Encoding.UTF8, "text/xml");
            using var response = await _httpClient.PostAsync(AuthUrl, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var xml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ParseAuthToken(xml);
        }

        public async Task DownloadAsync(string authToken, string outputFolder, string rfc, string startDate, string endDate)
        {
            if (string.IsNullOrWhiteSpace(authToken))
                throw new ArgumentException("Authentication token is required", nameof(authToken));
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            var soap = BuildDownloadEnvelope(authToken, rfc, startDate, endDate);
            var content = new StringContent(soap, Encoding.UTF8, "text/xml");
            using var response = await _httpClient.PostAsync(DownloadUrl, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var zipData = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var outFile = Path.Combine(outputFolder, $"cfdi_{startDate}_{endDate}.zip");
            await File.WriteAllBytesAsync(outFile, zipData).ConfigureAwait(false);
        }

        private static string BuildAuthEnvelope(X509Certificate2 cert, string rfc)
        {
            var encodedCert = Convert.ToBase64String(cert.RawData);
            var envelope = new XDocument(
                new XElement("Autenticacion",
                    new XElement("rfc", rfc),
                    new XElement("certificado", encodedCert))
            );
            return envelope.ToString();
        }

        private static string ParseAuthToken(string xml)
        {
            var doc = XDocument.Parse(xml);
            var token = doc.Root?.Element("token")?.Value;
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidDataException("Token not present in response");
            return token;
        }

        private static string BuildDownloadEnvelope(string token, string rfc, string start, string end)
        {
            var envelope = new XDocument(
                new XElement("SolicitudDescarga",
                    new XElement("token", token),
                    new XElement("rfcSolicitante", rfc),
                    new XElement("fechaInicial", start),
                    new XElement("fechaFinal", end))
            );
            return envelope.ToString();
        }
    }
}
