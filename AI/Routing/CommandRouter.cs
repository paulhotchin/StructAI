using StructAI.AI.Commands;

namespace StructAI.AI.Routing;

public class CommandRouter {
    public Task<object> ExecuteAsync(AiCommand cmd) {
        return cmd.Command switch {
            "create_project" => CreateProject(cmd.Parameters),
            "validate_project" => ValidateProject(cmd.Parameters),
            "analyse_loads" => AnalyseLoads(cmd.Parameters),
            _ => Task.FromResult<object>($"Unknown command: {cmd.Command}")
        };
    }

    private Task<object> CreateProject(Dictionary<string, object> p) {
        // TODO: plug into your real project creation logic
        var result = new {
            Status = "created",
            Name = p.GetValueOrDefault("name"),
            Address = p.GetValueOrDefault("address"),
            Engineer = p.GetValueOrDefault("engineer")
        };

        return Task.FromResult<object>(result);
    }

    private Task<object> ValidateProject(Dictionary<string, object> p) {
        // TODO: real validation
        var result = new {
            Status = "validated",
            ProjectId = p.GetValueOrDefault("projectId")
        };

        return Task.FromResult<object>(result);
    }

    private Task<object> AnalyseLoads(Dictionary<string, object> p) {
        // TODO: real load analysis
        var result = new {
            Status = "analysed",
            ProjectId = p.GetValueOrDefault("projectId"),
            LoadCase = p.GetValueOrDefault("loadCase")
        };

        return Task.FromResult<object>(result);
    }
}
