namespace StructAI.AI.Commands;

public class AiCommand {
    public string Command { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}
