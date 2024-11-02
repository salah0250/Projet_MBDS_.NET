using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;

namespace Gauniv.Client.Services
{
    internal partial class NetworkService : ObservableObject
    {
        public static NetworkService Instance { get; private set; } = new NetworkService();

        [ObservableProperty]
        private string token;

        public HttpClient httpClient;

        public NetworkService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            Token = null;
        }

        public void SetAuthToken(string token)
        {
            Token = token;
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                httpClient.DefaultRequestHeaders.Authorization = null;
            }
            OnConnected?.Invoke();
        }

        public event Action OnConnected;
    }
}
