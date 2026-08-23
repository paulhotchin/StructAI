using System.Text;
using StructAI.AI.Commands;

namespace StructAI.AI.Prompting;

public class PromptEngine {
    private readonly CommandSchema _schema;

    public PromptEngine(CommandSchema schema) {
        _schema = schema;
    }

    public string BuildPrompt(string userInput) {
        var sb = new StringBuilder();

        sb.AppendLine("You are an AI command interpreter.");
        sb.AppendLine();
        sb.AppendLine("Available commands:");

        foreach (var c in _schema.Commands) {
            sb.AppendLine($"- {c.Name}: {c.Description} (params: {string.Join(", ", c.Parameters)})");
        }

        sb.AppendLine();
        sb.AppendLine($"User input: {userInput}");
        sb.AppendLine();
        sb.AppendLine("Respond ONLY in JSON:");
        sb.AppendLine(@"
{
  ""command"": ""<command_name>"",
  ""parameters"": { <key>: <value> }
}");

        return sb.ToString();
    }
}
