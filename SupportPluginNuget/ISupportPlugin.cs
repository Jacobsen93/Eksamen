using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace SupportPluginNuget
{
    public interface ISupportPlugin
    {
        Task InitializeAsync(string apiEndpoint, string fullComponentName, IServiceCollection services);
        Type GetSelectedComponent();
        bool IsLoading { get; }
        string ErrorMessage { get; }
    }

    public class SupportPlugin : ISupportPlugin
    {
        private readonly HttpClient _httpClient;

        public SupportPlugin(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public bool IsLoading { get; private set; } = true;
        public List<Type> Components { get; private set; } = new();
        public Type SelectedComponent { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;

        public async Task InitializeAsync(string apiEndpoint, string fullComponentName, IServiceCollection services)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

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

                        // Register services from the loaded assembly
                        RegisterServicesFromAssembly(assembly, services);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        var loadedTypes = ex.Types.Where(type => type != null && IsBlazorComponent(type));
                        Components.AddRange(loadedTypes);
                    }
                }

                // Set the selected component based on the full component name
                SelectedComponent = Components.FirstOrDefault(c => c.FullName == fullComponentName);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error initializing components: {ex.Message}";
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
                ErrorMessage = $"Error loading assembly from {url}: {ex.Message}";
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

        private void RegisterServicesFromAssembly(Assembly assembly, IServiceCollection services)
        {
            // Register Blazor Bootstrap services if the assembly contains them
            var bootstrapServiceType = assembly.GetType("BlazorBootstrap.ServiceCollectionExtensions");
            if (bootstrapServiceType != null)
            {
                var method = bootstrapServiceType.GetMethod("AddBlazorBootstrap", new[] { typeof(IServiceCollection) });
                method?.Invoke(null, new object[] { services });
            }
        }

        public Type GetSelectedComponent()
        {
            return SelectedComponent;
        }

        public class ComponentResponse
        {
            public List<string> Components { get; set; }
        }
    }
}
