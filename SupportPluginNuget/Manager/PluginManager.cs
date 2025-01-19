namespace SupportPluginNuget.Manager
{
    public class PluginManager
    {
        private readonly HttpClient _httpClient;

        public PluginManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetSupportInfoFromPluginAsync(string pluginUrl)
        {
            // Call the plugin's REST API endpoint
            try
            {
                var response = await _httpClient.GetAsync($"{pluginUrl}");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (HttpRequestException ex)
            {
                return $"Error calling plugin: {ex.Message}";
            }
        }
    }
}
