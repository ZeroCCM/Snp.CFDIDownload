using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
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
    }
}
