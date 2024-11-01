using System.Diagnostics;
using System.Net.Http.Json;
using Gauniv.Client.Data;
using Newtonsoft.Json;

public class GameService
{
    private readonly HttpClient _httpClient;

    public GameService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Game>> GetAllGamesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://localhost:59221/api/1.0.0/Games/GetAllGames");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Game>>(content);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error fetching games: {ex.Message}");
            return Enumerable.Empty<Game>();
        }
    }

}

