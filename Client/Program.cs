using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp.Client;
using BlazorApp.Client.Services;
using Blazored.LocalStorage;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddBlazoredLocalStorage(options => { options.JsonSerializerOptions.TypeInfoResolver = SerializationContext.Default; }); //Need to wait for https://github.com/Blazored/LocalStorage/pull/241
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddFluentUIComponents();

// Register notification service
builder.Services.AddScoped<NotificationService>();

await builder.Build().RunAsync();
