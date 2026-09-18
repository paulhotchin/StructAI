// E:\work\TQ\Kepler\StructAI\StructAI.App\Services\WizardEngine.cs
using Microsoft.JSInterop;
using StructAI.Model;
using StructAI.Wizard;
using System.Text.Json;

namespace StructAI.Services;

public class WizardEngine
{
    public WizardDefinition? Definition { get; private set; }
    public object? Model { get; private set; }
    public int CurrentStepIndex { get; private set; } = 0;

    private readonly JsonSerializerOptions _jsonOptions =
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    private readonly IJSRuntime _js;

    public WizardEngine(IJSRuntime js)
    {
        _js = js;
    }

    // ------------------------------------------------------------
    // ORIGINAL API (kept for MainLayout + Index)
    // ------------------------------------------------------------
    public async Task LoadMetadataAsync(HttpClient http)
    {
        // Default wizard = GroundFloor
        await LoadMetadataAsync(http, "AppData/metadata/GroundFloorWizard.json");
    }

    public async Task LoadModelAsync(HttpClient http)
    {
        // Default wizard = GroundFloor
        await LoadModelAsync(http, "AppData/models/GroundFloor.json");
    }

    public async Task LoadMetadataAsync(HttpClient http, string metadataPath)
    {
        var json = await http.GetStringAsync(metadataPath);

        Definition = JsonSerializer.Deserialize<WizardDefinition>(json, _jsonOptions)
            ?? throw new InvalidOperationException("WizardEngine: failed to deserialize metadata.");

        if (Definition.Steps == null || Definition.Steps.Count == 0)
            throw new InvalidOperationException("WizardEngine: no steps defined in metadata.");
    }

    public async Task LoadModelAsync(HttpClient http, string modelPath)
    {
        var savedJson = await _js.InvokeAsync<string?>(
            "structAI.load",
            "WizardModel");

        var json = string.IsNullOrWhiteSpace(savedJson)
            ? await http.GetStringAsync(modelPath)
            : savedJson;

        if (modelPath.Contains("GroundFloor", StringComparison.OrdinalIgnoreCase))
        {
            Model = JsonSerializer.Deserialize<GroundFloor>(json, _jsonOptions);
        }
        else if (modelPath.Contains("CeoValueTime", StringComparison.OrdinalIgnoreCase))
        {
            Model = JsonSerializer.Deserialize<CeoValueTime>(json, _jsonOptions);
        }
        else
        {
            Model = JsonSerializer.Deserialize<object>(json, _jsonOptions);
        }

        if (Model is null)
            throw new InvalidOperationException("WizardEngine: failed to load model.");
    }

    // ------------------------------------------------------------
    // COMPATIBILITY OVERLOAD FOR WIZARDPAGE
    // ------------------------------------------------------------
    public async Task LoadModelAsync(HttpClient http, string modelPath, string wizardId)
    {
        await LoadModelAsync(http, modelPath);   // call unified loader
        Compute(wizardId);                       // compute immediately
    }

    public async Task SaveModelAsync()
    {
        var json = JsonSerializer.Serialize(Model, new JsonSerializerOptions { WriteIndented = true });
        await _js.InvokeVoidAsync("structAI.save", "WizardModel", json);
    }

    // ------------------------------------------------------------
    // STEP NAVIGATION
    // ------------------------------------------------------------
    public WizardStep CurrentStep =>
        Definition?.Steps?[CurrentStepIndex]
        ?? throw new InvalidOperationException("WizardEngine: metadata not loaded.");

    public bool IsFirstStep => CurrentStepIndex <= 0;
    public bool IsLastStep => Definition?.Steps == null
        || CurrentStepIndex >= Definition.Steps.Count - 1;

    public void NextStep()
    {
        if (!IsLastStep)
            CurrentStepIndex++;
    }

    public void PreviousStep()
    {
        if (!IsFirstStep)
            CurrentStepIndex--;
    }

    // ------------------------------------------------------------
    // TYPED PATH RESOLUTION
    // ------------------------------------------------------------
    public T ResolvePath<T>(string path)
    {
        if (Model is null)
            throw new InvalidOperationException("WizardEngine: model not loaded.");

        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        object current = Model;

        foreach (var part in parts)
        {
            var prop = current.GetType().GetProperty(part)
                ?? throw new InvalidOperationException(
                    $"Property '{part}' not found on '{current.GetType().Name}'.");

            current = prop.GetValue(current)
                ?? throw new InvalidOperationException(
                    $"Property '{part}' on '{current.GetType().Name}' is null.");
        }

        if (current is T typed)
            return typed;

        throw new InvalidOperationException(
            $"Resolved value for path '{path}' is not of type '{typeof(T).Name}'.");
    }

    public bool TryResolvePath<T>(string path, out T? value)
    {
        value = default;

        if (Model is null || string.IsNullOrWhiteSpace(path))
            return false;

        object? current = Model;

        foreach (var part in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            var property = current?.GetType().GetProperty(part);
            if (property == null)
                return false;

            current = property.GetValue(current);

            if (current == null)
                return false;
        }

        if (current is T typed)
        {
            value = typed;
            return true;
        }

        return false;
    }

    // ------------------------------------------------------------
    // COMPUTE DISPATCHER
    // ------------------------------------------------------------
    public void Compute(string wizardId)
    {
        switch (wizardId)
        {
            case "GroundFloor":
                ComputeGroundFloor();
                break;

            case "CeoValueTime":
                ComputeCeoValueTime();
                break;
        }
    }

    // ------------------------------------------------------------
    // GROUND FLOOR COMPUTE
    // ------------------------------------------------------------
    private void ComputeGroundFloor()
    {
        var site = ResolvePath<Site>("Site");
        if (site.Boundary == null || site.Boundary.Count < 3)
            return;

        double perimeter = 0;
        double area = 0;

        var pts = site.Boundary;

        for (int i = 0; i < pts.Count; i++)
        {
            var p1 = pts[i];
            var p2 = pts[(i + 1) % pts.Count];

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            perimeter += Math.Sqrt(dx * dx + dy * dy);
            area += (p1.X * p2.Y) - (p2.X * p1.Y);
        }

        area = Math.Abs(area) / 2.0;

        site.BoundaryPerimeter = perimeter;
        site.BoundaryArea = area;
    }

    // ------------------------------------------------------------
    // CEO VALUE TIME COMPUTE
    // ------------------------------------------------------------
    private void ComputeCeoValueTime()
    {
        var ceo = Model as CeoValueTime;
        if (ceo == null)
            return;

        double hourly = ceo.HourlyRate;
        double adminHours = ceo.AdminHours;
        double interruptions = ceo.InterruptionsPerDay;
        double minutesLost = ceo.MinutesLostPerInterruption;

        ceo.WeeklyCostAdmin =
            adminHours * hourly;

        ceo.WeeklyCostInterruptions =
            interruptions * (minutesLost / 60.0) * hourly * 5;

        ceo.TotalWeeklyLoss =
            ceo.WeeklyCostAdmin + ceo.WeeklyCostInterruptions;

        ceo.AnnualLoss =
            ceo.TotalWeeklyLoss * 52;
    }

    public bool TrySetPath<T>(string path, T value)
    {
        if (Model is null || string.IsNullOrWhiteSpace(path))
            return false;

        object current = Model;
        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < parts.Length - 1; i++)
        {
            var property = current.GetType().GetProperty(parts[i]);

            if (property?.GetValue(current) is not object next)
                return false;

            current = next;
        }

        var targetProperty = current.GetType().GetProperty(parts[^1]);

        if (targetProperty is null || !targetProperty.CanWrite)
            return false;

        targetProperty.SetValue(current, value);
        return true;
    }

    public event Action? ModelChanged;

    public void NotifyModelChanged()
    {
        ModelChanged?.Invoke();
    }
}
