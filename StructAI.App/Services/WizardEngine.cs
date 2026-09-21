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
    public int CurrentStepIndex { get; private set; }

    private readonly IJSRuntime _js;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private string _wizardId = "GroundFloor";

    public WizardEngine(IJSRuntime js)
    {
        _js = js;
    }

    private string StorageKey =>
        $"WizardModel_{_wizardId}";

    public async Task LoadMetadataAsync(HttpClient http)
    {
        await LoadMetadataAsync(
            http,
            "AppData/metadata/GroundFloorWizard.json");
    }

    public async Task LoadModelAsync(HttpClient http)
    {
        await LoadModelAsync(
            http,
            "AppData/models/GroundFloor.json");
    }

    public async Task LoadMetadataAsync(
        HttpClient http,
        string metadataPath)
    {
        var json = await http.GetStringAsync(metadataPath);

        Definition = JsonSerializer.Deserialize<WizardDefinition>(
            json,
            _jsonOptions)
            ?? throw new InvalidOperationException(
                "WizardEngine: failed to deserialize metadata.");

        if (Definition.Steps is null || Definition.Steps.Count == 0)
        {
            throw new InvalidOperationException(
                "WizardEngine: no steps defined in metadata.");
        }

        Definition.AttachEngine(this);
        _wizardId = GetWizardId(metadataPath);
        CurrentStepIndex = 0;
    }

    public async Task LoadModelAsync(
        HttpClient http,
        string modelPath)
    {
        // Set the key from the model path even when metadata was not loaded first.
        _wizardId = GetWizardId(modelPath);

        var savedJson = await _js.InvokeAsync<string?>(
            "structAI.load",
            StorageKey);

        var json = string.IsNullOrWhiteSpace(savedJson)
            ? await http.GetStringAsync(modelPath)
            : savedJson;

        Model = _wizardId switch
        {
            "GroundFloor" =>
                JsonSerializer.Deserialize<GroundFloor>(
                    json,
                    _jsonOptions),

            "CeoValueTime" =>
                JsonSerializer.Deserialize<CeoValueTime>(
                    json,
                    _jsonOptions),

            "StructAiNewProject" =>
                JsonSerializer.Deserialize<StructAiNewProject>(
                    json,
                    _jsonOptions),

            "CicSalesProjection" =>
                JsonSerializer.Deserialize<CicSalesProjection>(
                    json,
                    _jsonOptions),

            _ =>
                JsonSerializer.Deserialize<object>(
                    json,
                    _jsonOptions)
        };

        if (Model is CicSalesProjection cic)
        {
            cic.EnsureRequiredItems();
        }

        if (Model is null)
        {
            throw new InvalidOperationException(
                $"WizardEngine: failed to load model '{_wizardId}'.");
        }

        Compute(_wizardId);
        NotifyModelChanged();
    }

    public async Task LoadModelAsync(
        HttpClient http,
        string modelPath,
        string wizardId)
    {
        _wizardId = wizardId;
        await LoadModelAsync(http, modelPath);
    }

    public async Task SaveModelAsync()
    {
        if (Model is null)
            throw new InvalidOperationException(
                "WizardEngine: model not loaded.");

        Compute(_wizardId);

        var json = JsonSerializer.Serialize(
            Model,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        await _js.InvokeVoidAsync(
            "structAI.save",
            StorageKey,
            json);
    }

    private static string GetWizardId(string path)
    {
        if (path.Contains(
                "CeoValueTime",
                StringComparison.OrdinalIgnoreCase))
        {
            return "CeoValueTime";
        }

        if (path.Contains(
                "GroundFloor",
                StringComparison.OrdinalIgnoreCase))
        {
            return "GroundFloor";
        }

        if (path.Contains(
                "StructAiNewProject",
                StringComparison.OrdinalIgnoreCase))
        {
            return "StructAiNewProject";
        }

        if (path.Contains(
                "CicSalesProjection",
                StringComparison.OrdinalIgnoreCase))
        {
            return "CicSalesProjection";
        }

        return "Default";
    }

    public WizardStep CurrentStep =>
        Definition?.Steps?[CurrentStepIndex]
        ?? throw new InvalidOperationException(
            "WizardEngine: metadata not loaded.");

    public void BeginLoad()
    {
        Definition = null;
        Model = null;
        CurrentStepIndex = 0;
        NotifyModelChanged();
    }

    public bool IsFirstStep => CurrentStepIndex <= 0;

    public bool IsLastStep =>
        Definition?.Steps is null ||
        CurrentStepIndex >= Definition.Steps.Count - 1;

    public void NextStep()
    {
        if (!IsLastStep)
        {
            CurrentStepIndex++;
            Compute(_wizardId);
            NotifyModelChanged();
        }
    }

    public void PreviousStep()
    {
        if (!IsFirstStep)
        {
            CurrentStepIndex--;
            Compute(_wizardId);
            NotifyModelChanged();
        }
    }

    public T ResolvePath<T>(string path)
    {
        if (!TryResolvePath<T>(path, out var value))
        {
            throw new InvalidOperationException(
                $"Unable to resolve path '{path}'.");
        }

        return value!;
    }

    public bool TryResolvePath<T>(
        string path,
        out T? value)
    {
        value = default;

        if (Model is null || string.IsNullOrWhiteSpace(path))
            return false;

        object? current = Model;

        foreach (var part in path.Split(
                     '.',
                     StringSplitOptions.RemoveEmptyEntries))
        {
            var property = current?.GetType().GetProperty(part);

            if (property is null)
                return false;

            current = property.GetValue(current);

            if (current is null)
                return false;
        }

        if (current is T typed)
        {
            value = typed;
            return true;
        }

        return false;
    }

    public bool TrySetPath<T>(
        string path,
        T value)
    {
        if (Model is null || string.IsNullOrWhiteSpace(path))
            return false;

        object current = Model;

        var parts = path.Split(
            '.',
            StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < parts.Length - 1; i++)
        {
            var property = current.GetType().GetProperty(parts[i]);

            if (property?.GetValue(current) is not object next)
                return false;

            current = next;
        }

        var target = current.GetType().GetProperty(parts[^1]);

        if (target is null || !target.CanWrite)
            return false;

        target.SetValue(current, value);
        Compute(_wizardId);
        NotifyModelChanged();

        return true;
    }

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

            case "StructAiNewProject":
                break;

            case "CicSalesProjection":
                if (Model is CicSalesProjection cic)
                {
                    if (cic.PriceItems.Count == 0)
                    {
                        cic.PriceItems = new List<CicPriceItem>
                        {
                            new() { Name = "Assessment", Price = cic.AssessmentPrice },
                            new() { Name = "Debrief", Price = cic.DebriefPrice },
                            new() { Name = "ADV & Debrief", Price = cic.AdvDebriefPrice },
                            new() { Name = "ADV pilot of 3", Price = cic.AdvPilotThreePrice },
                            new() { Name = "ADV pilot of 6", Price = cic.AdvPilotSixPrice }
                        };
                    }

                    cic.SyncLegacyPrices();
                }
                break;
        }
    }

    private void ComputeGroundFloor()
    {
        if (Model is not GroundFloor groundFloor ||
            groundFloor.Site?.Boundary is not { Count: >= 3 } points)
        {
            return;
        }

        double perimeter = 0;
        double area = 0;

        for (var i = 0; i < points.Count; i++)
        {
            var first = points[i];
            var second = points[(i + 1) % points.Count];

            var dx = second.X - first.X;
            var dy = second.Y - first.Y;

            perimeter += Math.Sqrt(dx * dx + dy * dy);
            area += first.X * second.Y - second.X * first.Y;
        }

        groundFloor.Site.BoundaryPerimeter = perimeter;
        groundFloor.Site.BoundaryArea = Math.Abs(area) / 2;
    }

    private void ComputeCeoValueTime()
    {
        if (Model is not CeoValueTime ceo)
            return;

        ceo.AnnualHours =
            ceo.WeeksPerYear * ceo.HoursPerWeek;

        ceo.AnnualValue =
            ceo.AnnualHours;

        ceo.AnnualValueWithGrowth =
            ceo.AnnualValue *
            (1 + ceo.AnnualGrowthPercent / 100);
    }

    public event Action? ModelChanged;

    public void NotifyModelChanged()
    {
        ModelChanged?.Invoke();
    }
}
