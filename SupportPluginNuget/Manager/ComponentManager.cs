using System.Net.Http.Json;
using System.Reflection;

namespace SupportPluginTestApp.Manager
{
    public class ComponentManager
    {
        private readonly HttpClient _httpClient;

        public ComponentManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public bool IsLoading { get; private set; } = true;
        public List<Type> Components { get; private set; } = new();
        public Type SelectedComponent { get; private set; }

        public async Task InitializeAsync(string apiEndpoint, string defaultComponentFullName)
        {
            try
            {
                IsLoading = true;

                // Fetch metadata from the API
                var response = await _httpClient.GetFromJsonAsync<ComponentResponse>(apiEndpoint);
                var componentUrls = response?.Components ?? new List<string>();

                foreach (var url in componentUrls)
                {
                    var assembly = await LoadAssemblyFromUrl(url);
                    if (assembly == null) continue;

                    try
                    {
                        // Add Blazor components dynamically
                        var types = assembly.GetTypes().Where(IsBlazorComponent);
                        Components.AddRange(types);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        var loadedTypes = ex.Types.Where(type => type != null && IsBlazorComponent(type));
                        Components.AddRange(loadedTypes);
                    }
                }

                // Set the default selected component
                SelectedComponent = Components.FirstOrDefault(type => type.FullName == defaultComponentFullName);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<Assembly> LoadAssemblyFromUrl(string url)
        {
            try
            {
                var assemblyBytes = await _httpClient.GetByteArrayAsync(url);
                return Assembly.Load(assemblyBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading assembly from {url}: {ex.Message}");
                return null;
            }
        }

        private bool IsBlazorComponent(Type type)
        {
            if (!type.IsPublic || type.IsAbstract)
                return false;

            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.FullName == "Microsoft.AspNetCore.Components.ComponentBase")
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }

            return false;
        }

        public class ComponentResponse
        {
            public List<string> Components { get; set; }
        }
    }
}
