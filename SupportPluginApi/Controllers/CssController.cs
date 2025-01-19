using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace SupportPluginApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CssController : ControllerBase
    {
        public string AssemblyFilePath { get; set; }
        public string EmbeddedCssResourceName { get; set; } = "SupportPluginRazorClassLibrary.wwwroot.css.custom-styles.css";

        private readonly Assembly _assembly;
        private readonly Func<string, Assembly> _assemblyLoader;

        public CssController(Assembly assembly = null, Func<string, Assembly> assemblyLoader = null)
        {
            _assembly = assembly;
            _assemblyLoader = assemblyLoader ?? Assembly.LoadFrom;
            string userRootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            // Set the AssemblyFilePath using a relative path
            AssemblyFilePath = Path.Combine(userRootPath, "source\\repos\\Eksamen\\SupportPluginRazorClassLibrary\\bin\\Debug\\net8.0\\SupportPluginRazorClassLibrary.dll");
        }

        [HttpGet("get-css")]
        public async Task<IActionResult> GetCssFile()
        {
            Assembly assembly = _assembly;

            if (assembly == null)
            {
                if (!System.IO.File.Exists(AssemblyFilePath))
                    return NotFound(new { error = "Assembly file not found" });

                try
                {
                    assembly = _assemblyLoader(AssemblyFilePath);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { error = $"Failed to load assembly: {ex.Message}" });
                }
            }

            using var resourceStream = assembly.GetManifestResourceStream(EmbeddedCssResourceName);
            if (resourceStream == null)
                return NotFound(new { error = "Embedded CSS file not found in the assembly" });

            try
            {
                using var reader = new StreamReader(resourceStream, Encoding.UTF8);
                var cssContent = await reader.ReadToEndAsync();
                return Content(cssContent, "text/css", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An unexpected error occurred: {ex.Message}" });
            }
        }
    }
}
