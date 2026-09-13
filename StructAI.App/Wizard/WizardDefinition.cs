namespace StructAI.Wizard;

public class WizardDefinition {
    public string? Name { get; set; }
    public List<WizardStep> Steps { get; set; } = new();
}

public class WizardStep {
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<WizardField> Fields { get; set; } = new();
}

public class WizardField {
    public string? Name { get; set; }
    public string? Label { get; set; }
    public string? Type { get; set; }
    public string? Bind { get; set; }
    public object? Default { get; set; }
    public FieldValidation? Validation { get; set; }
    public FieldUI? UI { get; set; }
}

public class FieldValidation {
    public double? Min { get; set; }
    public double? Max { get; set; }
    public int? MinCount { get; set; }
}

public class FieldUI {
    public string? Hint { get; set; }
    public string? Editor { get; set; }
}
