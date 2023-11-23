using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ACB.II.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AutoRegister();
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    options.WriteIndented = false;
    options.Converters.Add(new JsonStringEnumConverter());
    options.IgnoreReadOnlyProperties = false;
});

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();