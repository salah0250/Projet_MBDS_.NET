using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Gauniv.Client.DTOs;

namespace Gauniv.Client.Services
{
    public interface IProfileService
    {
        Task<UserDto> GetUserProfileAsync();
        Task<UserDto> UpdateProfileAsync(string email, string firstName, string lastName, string phoneNumber);
    }

    public class ProfileService : IProfileService
    {
        private readonly NetworkService _networkService;

        public ProfileService()
        {
            _networkService = NetworkService.Instance;
        }

        public async Task<UserDto> GetUserProfileAsync()
        {
            try
            {
                var response = await _networkService.httpClient.GetAsync("/api/1.0.0/Users/GetUserProfile");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<UserDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to fetch profile data", ex);
            }
        }

        public async Task<UserDto> UpdateProfileAsync(string email, string firstName, string lastName, string phoneNumber)
        {
            try
            {
                var updateData = new { Email = email, FirstName = firstName, LastName = lastName, PhoneNumber = phoneNumber };
                var response = await _networkService.httpClient.PutAsJsonAsync("/api/1.0.0/Users/UpdateProfile", updateData);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<UserDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to update profile", ex);
            }
        }
    }


}
