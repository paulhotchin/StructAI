using System.Text.Json;
using StructAI.Wizard;
using StructAI.Model;

namespace StructAI.Services;

public class WizardEngine {
    public WizardDefinition? Definition { get; private set; }
    public object? Model { get; private set; }
    public int CurrentStepIndex { get; private set; } = 0;

    private readonly JsonSerializerOptions _jsonOptions =
        new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        };

    public async Task LoadMetadataAsync(HttpClient http) {
        var json = await http.GetStringAsync("metadata/GroundFloorWizard.json");

        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("WizardEngine: metadata JSON is empty.");

        Definition = JsonSerializer.Deserialize<WizardDefinition>(json, _jsonOptions)
            ?? throw new InvalidOperationException("WizardEngine: failed to deserialize metadata.");

        if (Definition.Steps == null || Definition.Steps.Count == 0)
            throw new InvalidOperationException("WizardEngine: no steps defined in metadata.");
    }

    public async Task LoadModelAsync(HttpClient http) {
        var json = await http.GetStringAsync("models/GroundFloor.json");

        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("WizardEngine: model JSON is empty.");

        Model = JsonSerializer.Deserialize<GroundFloor>(json, _jsonOptions)
            ?? throw new InvalidOperationException("WizardEngine: failed to deserialize model.");
    }

    public WizardStep GetStep(int index) {
        if (Definition is null)
            throw new InvalidOperationException("WizardEngine: metadata not loaded.");

        if (Definition.Steps == null || Definition.Steps.Count == 0)
            throw new InvalidOperationException("WizardEngine: no steps defined.");

        if (index < 0 || index >= Definition.Steps.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var step = Definition.Steps[index];
        return step ?? throw new InvalidOperationException("WizardEngine: step is null.");
    }

    public WizardField GetField(WizardStep step, string fieldName) {
        if (step is null)
            throw new ArgumentNullException(nameof(step));

        var field = step.Fields.FirstOrDefault(f => f.Name == fieldName);

        return field ?? throw new InvalidOperationException(
            $"WizardEngine: field '{fieldName}' not found in step '{step.Name}'.");
    }

    public WizardStep CurrentStep {
        get {
            if (Definition is null)
                throw new InvalidOperationException("WizardEngine: metadata not loaded.");

            if (Definition.Steps == null || Definition.Steps.Count == 0)
                throw new InvalidOperationException("WizardEngine: no steps defined.");

            return Definition.Steps[CurrentStepIndex];
        }
    }

    public void NextStep() {
        if (Definition is null)
            throw new InvalidOperationException("WizardEngine: metadata not loaded.");

        if (Definition.Steps == null || Definition.Steps.Count == 0)
            throw new InvalidOperationException("WizardEngine: no steps defined.");

        if (CurrentStepIndex < Definition.Steps.Count - 1)
            CurrentStepIndex++;
    }

    public void PreviousStep() {
        if (Definition is null)
            throw new InvalidOperationException("WizardEngine: metadata not loaded.");

        if (Definition.Steps == null || Definition.Steps.Count == 0)
            throw new InvalidOperationException("WizardEngine: no steps defined.");

        if (CurrentStepIndex > 0)
            CurrentStepIndex--;
    }

    public object ResolvePath(string path) {
        if (Model is null)
            throw new InvalidOperationException("WizardEngine: model not loaded.");

        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is null or empty.", nameof(path));

        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        object current = Model;

        foreach (var part in parts) {
            var prop = current.GetType().GetProperty(part);
            if (prop is null)
                throw new InvalidOperationException(
                    $"Property '{part}' not found on '{current.GetType().Name}'.");

            current = prop.GetValue(current)
                ?? throw new InvalidOperationException(
                    $"Property '{part}' on '{current.GetType().Name}' is null.");
        }

        return current;
    }

    public T ResolvePath<T>(string path) {
        var value = ResolvePath(path);

        if (value is T typed)
            return typed;

        throw new InvalidOperationException(
            $"Resolved value for path '{path}' is not of type '{typeof(T).Name}'.");
    }
}
