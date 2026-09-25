using System.Text.Json;
using StructAI.Model;

namespace StructAI.Services;

public class AppSettingsService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private Task? _loadTask;

    public AppSettings? Settings { get; private set; }
    public event Action? SettingsChanged;

    public AppSettingsService(HttpClient http)
    {
        _http = http;
    }

    public Task LoadAsync()
    {
        return _loadTask ??= LoadCoreAsync();
    }

    private async Task LoadCoreAsync()
    {
        var json = await _http.GetStringAsync(
            $"AppData/metadata/AppSettings.json?cacheBust={Guid.NewGuid():N}");

        Settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions)
            ?? throw new InvalidOperationException(
                "AppSettings metadata deserialized to null.");

        SettingsChanged?.Invoke();
    }

    public IReadOnlyList<AppProject> EnabledProjects =>
        Settings?.Projects.Where(project => project.IsEnabled).ToList()
        ?? [];
}
