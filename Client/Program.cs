using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp.Client;
using Blazored.LocalStorage;
using Microsoft.FluentUI.AspNetCore.Components;
using BlazorApp.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["API_Prefix"] ?? builder.HostEnvironment.BaseAddress) });

//builder.Services.AddBlazoredLocalStorage(options => { options.JsonSerializerOptions.TypeInfoResolver = SerializationContext.Default; }); //Need to wait for https://github.com/Blazored/LocalStorage/pull/241
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddFluentUIComponents();

builder.Services.AddHttpClient();

builder.Services.AddScoped<IAuthService, AuthService>();

await builder.Build().RunAsync();
