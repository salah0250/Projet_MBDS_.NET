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
            // Supprimer le token ici si nécessaire.
        }

        public void SetToken(string token)
        {
            // Stocker le token de manière sécurisée
            IsConnected = true;
        }
    }
}
