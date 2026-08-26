using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StructAI;
using StructAI.AI.Commands;
using StructAI.AI.Model;
using StructAI.AI.Pipeline;
using StructAI.AI.Prompting;
using StructAI.AI.Routing;
using StructAI.Services;
using System.Text.Json;
// using Microsoft.AspNetCore.Components.Web.Extensions

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient for WASM
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<WizardEngine>();

// Load command schema JSON BEFORE Build()
var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var json = await http.GetStringAsync("metadata/command-schema.json");
var schema = JsonSerializer.Deserialize<CommandSchema>(json)!;

// Register AI services BEFORE Build()
builder.Services.AddSingleton(schema);
builder.Services.AddSingleton<PromptEngine>();
builder.Services.AddScoped<AiClient>();
builder.Services.AddSingleton<CommandRouter>();
builder.Services.AddScoped<CommandRunnerService>();

await builder.Build().RunAsync();
