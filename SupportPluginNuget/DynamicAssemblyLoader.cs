using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

public class DynamicAssemblyLoader
{
    private readonly HttpClient _httpClient;

    public DynamicAssemblyLoader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Assembly> LoadAssemblyFromUrlAsync(string url)
    {
        var assemblyBytes = await _httpClient.GetByteArrayAsync(url);
        return Assembly.Load(assemblyBytes);
    }

    public void RegisterServicesFromAssembly(Assembly assembly, IServiceCollection services)
    {
        // Register Blazor Bootstrap services if the assembly contains them
        var bootstrapServiceType = assembly.GetType("BlazorBootstrap.ServiceCollectionExtensions");
        if (bootstrapServiceType != null)
        {
            var method = bootstrapServiceType.GetMethod("AddBlazorBootstrap", new[] { typeof(IServiceCollection) });
            method?.Invoke(null, new object[] { services });
        }
    }
}
