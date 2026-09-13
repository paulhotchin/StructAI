namespace StructAI.AI.Model;

public class AiClient {
    private readonly HttpClient _httpClient;

    public AiClient(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    public async Task<AiResponse> SendAsync(string prompt) {
        // Temporary await to silence CS1998
        await Task.CompletedTask;

        // TODO: replace with your actual LLM API call
        // Example pseudo-call:
        //
        // var request = new { prompt = prompt };
        // var response = await _httpClient.PostAsJsonAsync("https://your-llm-endpoint", request);
        // var json = await response.Content.ReadAsStringAsync();
        //
        // return new AiResponse { RawJson = json };

        return new AiResponse {
            RawJson = @"{ ""command"": ""create_project"", ""parameters"": { ""name"": ""Test"", ""address"": ""123 St"", ""engineer"": ""Paul"" } }"
        };
    }
}
