using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Snp.CFDIDownload;

namespace Snp.CFDIDownload.Tests
{
    [TestClass]
    public class CFDIDownloadServiceTests
    {
        [TestMethod]
        public void CanCreateService()
        {
            var service = new CFDIDownloadService(new HttpClient());
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public async Task AuthenticateAsync_ReturnsToken()
        {
            var handler = new FakeHttpMessageHandler();
            handler.QueueResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<response><token>abc</token></response>")
            });
            var service = new CFDIDownloadService(new HttpClient(handler));
            var token = await service.AuthenticateAsync("AAA010101AAA", "cert.pfx", "key.key", "pass");
            Assert.AreEqual("abc", token);
        }

        [TestMethod]
        public void LoadFielCertificate_ReturnsCertificate()
        {
            var password = "12345";
            var certPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".cer");
            var keyPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".key");

            using (var rsa = RSA.Create(2048))
            {
                var req = new CertificateRequest("cn=test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
                File.WriteAllBytes(certPath, cert.Export(X509ContentType.Cert));
                var pkcs8 = rsa.ExportEncryptedPkcs8PrivateKey(password, new PbeParameters(PbeEncryptionAlgorithm.TripleDes3KeyPkcs12, HashAlgorithmName.SHA1, 2048));
                File.WriteAllBytes(keyPath, pkcs8);
            }

            var loaded = CFDIDownloadService.LoadFielCertificate(certPath, keyPath, password);
            Assert.IsTrue(loaded.HasPrivateKey);
        }
    }
}
