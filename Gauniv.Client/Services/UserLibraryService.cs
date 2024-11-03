using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Gauniv.Client.Data;

namespace Gauniv.Client.Services
{
    public class UserLibraryService
    {
        private readonly NetworkService _networkService;

        public UserLibraryService()
        {
            _networkService = NetworkService.Instance;
        }

        public async Task<IEnumerable<Game>> GetUserLibraryAsync()
        {
            try
            {
                var response = await _networkService.httpClient.GetAsync("/api/1.0.0/Users/GetUserLibrary");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Game>>() ?? new List<Game>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching user library: {ex.Message}");
                return Enumerable.Empty<Game>();
            }
        }
    }
}
