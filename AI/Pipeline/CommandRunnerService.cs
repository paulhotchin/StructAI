using System.Text.Json;
using System.Threading.Tasks;
using StructAI.AI.Commands;
using StructAI.AI.Model;
using StructAI.AI.Prompting;
using StructAI.AI.Routing;

namespace StructAI.AI.Pipeline;

public class CommandRunnerService {
    private readonly PromptEngine _promptEngine;
    private readonly AiClient _aiClient;
    private readonly CommandRouter _router;

    public CommandRunnerService(
        PromptEngine promptEngine,
        AiClient aiClient,
        CommandRouter router) {
        _promptEngine = promptEngine;
        _aiClient = aiClient;
        _router = router;
    }

    public async Task<object> RunAsync(string userInput) {
        var prompt = _promptEngine.BuildPrompt(userInput);
        var aiResponse = await _aiClient.SendAsync(prompt);

        var aiCommand = JsonSerializer.Deserialize<AiCommand>(aiResponse.RawJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (aiCommand == null)
            return "Failed to parse AI command.";

        var result = await _router.ExecuteAsync(aiCommand);
        return result;
    }
}
