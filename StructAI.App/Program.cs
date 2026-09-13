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

// AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
// {
//     Console.WriteLine("FIRST CHANCE EXCEPTION: " + eventArgs.Exception);
// };

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient for WASM
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Wizard engine
builder.Services.AddScoped<WizardEngine>();

// Load command schema JSON BEFORE Build()
// IMPORTANT: StructAI must contain: wwwroot/AppData/metadata/command-schema.json
var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var json = await http.GetStringAsync("AppData/metadata/command-schema.json");
var schema = JsonSerializer.Deserialize<CommandSchema>(json)
    ?? throw new Exception("Command schema JSON deserialized to null.");

builder.Services.AddSingleton<CommandSchema>(schema);

// Register AI services BEFORE Build()
builder.Services.AddSingleton<PromptEngine>();
builder.Services.AddScoped<AiClient>();
builder.Services.AddSingleton<CommandRouter>();
builder.Services.AddScoped<CommandRunnerService>();

 await builder.Build().RunAsync();
