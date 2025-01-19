using Microsoft.AspNetCore.Mvc;

namespace SupportPluginApi.Controllers
{

    //this is used to get the diffrent ui-components from a shared .dll file dynamically
    public class UIComponentsController : ControllerBase
    {
        private string _uiComponentDirectory { get; set; }  // Local directory path -> should be changed to point at a shared file locaion

        public UIComponentsController()
        {
            string userRootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _uiComponentDirectory = Path.Combine(userRootPath, "source\\repos\\FVF_SupportAssistant\\SupportPluginRazorClassLibrary\\bin\\Debug\\net8.0");
        }

        [HttpGet("ui-components")]
        public IActionResult GetUIComponents()
        {
            // Get the list of .dll files from the local directory
            var dllFiles = Directory.GetFiles(_uiComponentDirectory, "*.dll");

            // Return local URLs or file paths to the client (hosted on localhost)
            return Ok(new
            {
                Components = dllFiles.Select(file => $"https://localhost:7118/ui-components/{Path.GetFileName(file)}").ToList()
            });
        }

        [HttpGet("ui-components/{fileName}")]
        public IActionResult GetDllFile(string fileName)
        {
            var filePath = Path.Combine(_uiComponentDirectory, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream");
        }
    }
}
