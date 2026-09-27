using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp.Client;
using BlazorApp.Client.Services;
using Blazored.LocalStorage;
using Microsoft.FluentUI.AspNetCore.Components;
using BlazorApp.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage(options => { options.JsonSerializerOptions.TypeInfoResolver = SerializationContext.Default; });

builder.Services.AddFluentUIComponents(options => options.Toast.MaxToastCount = 10);
builder.Services.AddSingleton<PwaUpdateService>();
builder.Services.AddSingleton<PeriodicSyncService>();

builder.Services.AddScoped(sp => new HttpClient
{
	BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Add browser identity service
builder.Services.AddScoped<IBrowserIdentityService, BrowserIdentityService>();
builder.Services.AddScoped<IAppSettingsService, AppSettingsService>();
builder.Services.AddScoped<ISyncEventService, SyncEventService>();

var app = builder.Build();

// Initialize browser identity on startup
var identityService = app.Services.GetRequiredService<IBrowserIdentityService>();
await identityService.GetOrCreateIdentityAsync();

await app.RunAsync();
