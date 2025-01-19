using SupportPluginTestApp.Components;
using SupportPluginTestApp.Manager;
using SupportPluginNuget;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using SupportPluginNuget.Manager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorBootstrap();
builder.Services.AddHttpClient<PluginManager>();

// Register the ISupportPlugin service
builder.Services.AddScoped<ISupportPlugin, SupportPlugin>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
// Get the base URI for local file placement
var environment = app.Services.GetRequiredService<IWebHostEnvironment>();
var baseUri = environment.WebRootPath;
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
