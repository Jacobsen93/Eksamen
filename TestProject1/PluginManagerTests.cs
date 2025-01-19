using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using SupportPluginNuget.Manager;
using SupportPluginTestApp.Manager;
using Xunit;

namespace SupportPluginTestApp.Tests
{
    public class PluginManagerTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _pluginManager = new PluginManager(_httpClient);
        }

        [Fact]
        public async Task GetSupportInfoFromPluginAsync_ReturnsContent_WhenResponseIsSuccessful()
        {
            // Arrange
            var pluginUrl = "http://example.com/plugin";
            var expectedContent = "Plugin support info";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedContent)
                });

            // Act
            var result = await _pluginManager.GetSupportInfoFromPluginAsync(pluginUrl);

            // Assert
            Assert.Equal(expectedContent, result);
        }

        [Fact]
        public async Task GetSupportInfoFromPluginAsync_ReturnsErrorMessage_WhenHttpRequestExceptionIsThrown()
        {
            // Arrange
            var pluginUrl = "http://example.com/plugin";
            var expectedErrorMessage = "Error calling plugin: Request failed";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Request failed"));

            // Act
            var result = await _pluginManager.GetSupportInfoFromPluginAsync(pluginUrl);

            // Assert
            Assert.Equal(expectedErrorMessage, result);
        }

        [Fact]
        public async Task GetSupportInfoFromPluginAsync_ReturnsErrorMessage_WhenResponseIsNotSuccessful()
        {
            // Arrange
            var pluginUrl = "http://example.com/plugin";
            var expectedErrorMessage = "Error calling plugin: Response status code does not indicate success: 500 (Internal Server Error).";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            var result = await _pluginManager.GetSupportInfoFromPluginAsync(pluginUrl);

            // Assert
            Assert.Equal(expectedErrorMessage, result);
        }
    }
}
