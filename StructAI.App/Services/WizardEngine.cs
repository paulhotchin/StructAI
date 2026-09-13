// E:\work\TQ\Kepler\StructAI\StructAI.App\Services\WizardEngine.cs
using Microsoft.JSInterop;
using StructAI.Model;
using StructAI.Wizard;
using System.Text.Json;

namespace StructAI.Services;

public class WizardEngine {
    public WizardDefinition? Definition { get; private set; }
    public object? Model { get; private set; }
    public int CurrentStepIndex { get; private set; } = 0;

    private readonly JsonSerializerOptions _jsonOptions =
        new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        };

    private readonly IJSRuntime _js;

    public WizardEngine(IJSRuntime js) {
        _js = js;
    }

    public async Task LoadMetadataAsync(HttpClient http) {
        var json = await http.GetStringAsync("AppData/metadata/GroundFloorWizard.json");

        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("WizardEngine: metadata JSON is empty.");

        Definition = JsonSerializer.Deserialize<WizardDefinition>(json, _jsonOptions)
            ?? throw new InvalidOperationException("WizardEngine: failed to deserialize metadata.");

        if (Definition.Steps == null || Definition.Steps.Count == 0)
            throw new InvalidOperationException("WizardEngine: no steps defined in metadata.");
    }
    public async Task LoadModelAsync(HttpClient http) {
        // Check if JS is ready
        var isReady = await _js.InvokeAsync<bool>("Boolean", "window.structAI !== undefined");

        if (!isReady) {
            // JS not ready yet → load default model
            var json = await http.GetStringAsync("AppData/models/GroundFloor.json");
            Model = JsonSerializer.Deserialize<GroundFloor>(json);
            return;
        }

        var saved = await _js.InvokeAsync<string>("structAI.load", "GroundFloorModel");

        if (!string.IsNullOrEmpty(saved)) {
            Model = JsonSerializer.Deserialize<GroundFloor>(saved);
            return;
        }

        var defaultJson = await http.GetStringAsync("AppData/models/GroundFloor.json");
        Model = JsonSerializer.Deserialize<GroundFloor>(defaultJson);
    }

    public async Task SaveModelAsync() {
        var json = JsonSerializer.Serialize(Model, new JsonSerializerOptions {
            WriteIndented = true
        });

        await _js.InvokeVoidAsync("structAI.save", "GroundFloorModel", json);
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
    public bool IsFirstStep => CurrentStepIndex <= 0;
    public bool IsLastStep => Definition?.Steps == null ? true : CurrentStepIndex >= Definition.Steps.Count - 1;

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
