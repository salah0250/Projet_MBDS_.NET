using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauniv.Client.Services
{

    public class AuthService
    {
        private bool _isConnected;
        private readonly NetworkService _networkService;

        public AuthService()
        {
            _networkService = NetworkService.Instance;
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler OnConnectionStatusChanged;

        public void Logout()
        {
            IsConnected = false;
            _networkService.SetAuthToken(null);
        }

        public void SetToken(string token)
        {
            _networkService.SetAuthToken(token);
            IsConnected = true;
        }

        public HttpClient GetHttpClient()
        {
            return _networkService.httpClient;
        }
    }
}
