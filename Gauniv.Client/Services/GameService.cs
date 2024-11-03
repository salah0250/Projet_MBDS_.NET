using Gauniv.Client.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using Gauniv.Client.Data;
public class GameService
{
    private readonly NetworkService _networkService;

    public GameService()
    {
        _networkService = NetworkService.Instance;
    }

    public async Task<IEnumerable<Game>> GetFilteredGamesAsync(string searchTerm, decimal? minPrice, decimal? maxPrice, string category)
    {
        try
        {
            var uri = $"/api/1.0.0/Games/GetFilteredGames/filter?searchTerm={searchTerm}&minPrice={minPrice}&maxPrice={maxPrice}&category={category}";
            var response = await _networkService.httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Game>>(content);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error fetching filtered games: {ex.Message}");
            return Enumerable.Empty<Game>();
        }
    }


    public async Task<IEnumerable<Game>> GetAllGamesAsync()
    {
        try
        {
            var response = await _networkService.httpClient.GetAsync("/api/1.0.0/Games/GetAllGames");
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


    // Ajout de la méthode d'achat
    public async Task<bool> PurchaseGameAsync(int gameId)
    {
        try
        {
            var response = await _networkService.httpClient.PostAsync($"/api/1.0.0/Games/PurchaseGame/{gameId}/purchase", null);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error purchasing game: {ex.Message}");
            return false;
        }
    }
}
