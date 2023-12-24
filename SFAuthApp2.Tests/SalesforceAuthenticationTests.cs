using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks; // Add this line
using Moq;
using Moq.Protected;
using System.Threading;


namespace SFAuthApp2.Tests
{
    [TestClass]
    public class SalesforceAuthenticationTests
    {
        private Mock<HttpMessageHandler> _httpClientHandlerMock;
        private HttpClient _httpClient;

        // Initialize mock objects and HttpClient before each test
        [TestInitialize]
        public void TestInitialize()
        {
            // Create a mock for HttpMessageHandler
            _httpClientHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpClientHandlerMock.Object);

            // Set the BaseAddress for HttpClient
            _httpClient.BaseAddress = new Uri("https://your-salesforce-instance-url/services/data/v57.0/query/?q=SELECT+Id,Name+FROM+Account"); // Replace with your actual Salesforce base URL

            // Set the HttpClient in the Program class to the mock HttpClient
            Program.HttpClient = _httpClient;
        }

        // Test for successful Salesforce authentication
        [TestMethod]
        public async Task TestSalesforceAuthentication_Success()
        {
            // Arrange
            _httpClientHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"access_token\":\"fakeAccessToken\"}")
                });

            // Act
            var result = await Program.AuthenticateSalesforce("fakeAuthEndpoint", "fakeClientId", "fakeClientSecret", "fakeUsername", "fakePassword");

            // Assert
            Assert.IsTrue(result);
        }

        // Test for failed Salesforce authentication
        [TestMethod]
        public async Task TestSalesforceAuthentication_Failure()
        {
            // Arrange
            _httpClientHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"error\":\"InvalidGrant\",\"error_description\":\"authentication failure\"}")
                });

            // Act
            var result = await Program.AuthenticateSalesforce("fakeAuthEndpoint", "fakeClientId", "fakeClientSecret", "fakeUsername", "fakePassword");

            // Assert
            Assert.IsFalse(result);
        }
    }
}
