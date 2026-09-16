// E:\work\TQ\Kepler\StructAI\StructAI.App\Wizard\WizardDefinition.cs
using StructAI.Services;

namespace StructAI.Wizard;

public class WizardDefinition
{
    public string? Name { get; set; }
    public List<WizardStep> Steps { get; set; } = new();

    public void AttachEngine(WizardEngine engine)
    {
        foreach (var step in Steps)
            foreach (var field in step.Fields)
                field.Engine = engine;
    }
}

public class WizardStep
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<WizardField> Fields { get; set; } = new();
}

public class WizardField
{
    public string Name { get; set; } = "";
    public string Label { get; set; } = "";
    public string Type { get; set; } = "";
    public string? Bind { get; set; }   // nullable
    public FieldUI? UI { get; set; }
    public List<FieldOption>? Options { get; set; }
    public FieldValidation? Validation { get; set; }

    public WizardEngine? Engine { get; set; }   // nullable

    // ------------------------------------------------------------
    // SAFE GET VALUE
    // ------------------------------------------------------------
    public T? GetValue<T>()
    {
        if (Engine == null || string.IsNullOrWhiteSpace(Bind))
            return default;

        try
        {
            return Engine.ResolvePath<T>(Bind);
        }
        catch
        {
            return default;
        }
    }

    // ------------------------------------------------------------
    // SAFE SET VALUE
    // ------------------------------------------------------------
    public void SetValue(object? value)
    {
        if (Engine == null || string.IsNullOrWhiteSpace(Bind))
            return;

        var parts = Bind.Split('.', StringSplitOptions.RemoveEmptyEntries);
        object? current = Engine.Model;   // FIXED: nullable

        if (current == null)
            return;

        try
        {
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var prop = current.GetType().GetProperty(parts[i]);
                if (prop == null)
                    return;

                current = prop.GetValue(current);   // FIXED: nullable assignment
                if (current == null)
                    return;
            }

            var last = parts.Last();
            var lastProp = current.GetType().GetProperty(last);
            if (lastProp == null)
                return;

            lastProp.SetValue(current, value);
        }
        catch
        {
            // swallow errors — invalid Bind path
        }
    }
}

public class FieldOption
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
}

public class FieldValidation
{
    public double? Min { get; set; }
    public double? Max { get; set; }
    public int? MinCount { get; set; }
}

public class FieldUI
{
    public string? Hint { get; set; }
    public string? Editor { get; set; }
}
