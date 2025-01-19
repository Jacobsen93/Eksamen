using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using SupportPluginApi.Controllers;
using Xunit;

namespace SupportPluginApi.Tests
{
    public class CssControllerTests
    {
        [Fact]
        public async Task GetCssFile_ReturnsNotFound_WhenAssemblyFileDoesNotExist()
        {
            // Arrange
            var controller = new CssController();
            controller.AssemblyFilePath = "nonexistent.dll";

            // Act
            var result = await controller.GetCssFile();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Assembly file not found", ((dynamic)notFoundResult.Value)?.error);
        }

        [Fact]
        public async Task GetCssFile_ReturnsInternalServerError_WhenAssemblyLoadFails()
        {
            // Arrange
            var assemblyLoaderMock = new Mock<Func<string, Assembly>>();
            assemblyLoaderMock.Setup(loader => loader(It.IsAny<string>())).Throws(new Exception("Load failure"));

            var controller = new CssController(assemblyLoader: assemblyLoaderMock.Object)
            {
                AssemblyFilePath = "invalid.dll"
            };

            // Act
            var result = await controller.GetCssFile();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("Failed to load assembly", ((dynamic)statusCodeResult.Value)?.error);
        }

        [Fact]
        public async Task GetCssFile_ReturnsNotFound_WhenEmbeddedCssResourceNotFound()
        {
            // Arrange
            var assemblyMock = new Mock<Assembly>();
            var controller = new CssController(assemblyMock.Object)
            {
                AssemblyFilePath = "valid.dll",
                EmbeddedCssResourceName = "nonexistent.css"
            };

            // Act
            var result = await controller.GetCssFile();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Embedded CSS file not found in the assembly", ((dynamic)notFoundResult.Value)?.error);
        }

        [Fact]
        public async Task GetCssFile_ReturnsCssContent_WhenSuccessful()
        {
            // Arrange
            var assemblyMock = new Mock<Assembly>();
            var resourceStream = new MemoryStream(Encoding.UTF8.GetBytes("body { background-color: #fff; }"));
            assemblyMock.Setup(a => a.GetManifestResourceStream(It.IsAny<string>())).Returns(resourceStream);

            var controller = new CssController(assemblyMock.Object)
            {
                AssemblyFilePath = "valid.dll",
                EmbeddedCssResourceName = "SupportPluginRazorClassLibrary.wwwroot.css.custom-styles.css"
            };

            // Act
            var result = await controller.GetCssFile();

            // Assert
            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal("text/css; charset=utf-8", contentResult.ContentType);
            Assert.Equal("body { background-color: #fff; }", contentResult.Content);
        }

        [Fact]
        public async Task GetCssFile_ReturnsInternalServerError_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var assemblyMock = new Mock<Assembly>();
            assemblyMock.Setup(a => a.GetManifestResourceStream(It.IsAny<string>())).Throws(new IOException("Unexpected error"));

            var controller = new CssController(assemblyMock.Object)
            {
                AssemblyFilePath = "valid.dll",
                EmbeddedCssResourceName = "SupportPluginRazorClassLibrary.wwwroot.css.custom-styles.css"
            };

            // Act
            var result = await controller.GetCssFile();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("An unexpected error occurred", ((dynamic)statusCodeResult.Value)?.error);
        }
    }
}
