using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp.Client;
using BlazorApp.Client.Services;
using Blazored.LocalStorage;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddBlazoredLocalStorage(options => { options.JsonSerializerOptions.TypeInfoResolver = SerializationContext.Default; }); //Need to wait for https://github.com/Blazored/LocalStorage/pull/241
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddFluentUIComponents();
builder.Services.AddSingleton<PwaUpdateService>();

builder.Services.AddScoped(sp => new HttpClient
{
	BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Add browser identity service
builder.Services.AddScoped<IBrowserIdentityService, BrowserIdentityService>();

var app = builder.Build();

// Initialize browser identity on startup
var identityService = app.Services.GetRequiredService<IBrowserIdentityService>();
await identityService.GetOrCreateIdentityAsync();

await app.RunAsync();
