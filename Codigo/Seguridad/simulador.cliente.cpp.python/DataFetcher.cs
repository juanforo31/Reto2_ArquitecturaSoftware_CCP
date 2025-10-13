using System.Net.Http;
using System.Text.Json;

public class DataFetcher
{
    private readonly HttpClient _client = new();

    public async Task<List<string>> FetchDataAsync(AppConfig config)
    {
        var results = new List<string>();
        var endTime = DateTime.Now.AddSeconds(config.DurationSeconds);

        while (DateTime.Now < endTime)
        {
            try
            {
                var response = await _client.GetStringAsync(config.Url);
                results.Add(response);
                Console.WriteLine($"[{DateTime.Now}] OK");
            }
            catch (Exception ex)
            {
                results.Add($"Error: {ex.Message}");
                Console.WriteLine($"[{DateTime.Now}] Error: {ex.Message}");
            }

            await Task.Delay(config.IntervalSeconds * 1000);
        }

        return results;
    }
}