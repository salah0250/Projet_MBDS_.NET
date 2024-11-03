using System.Diagnostics;
using System.Text.Json;
using Gauniv.Client.Settings;

namespace Gauniv.Client.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;
        private AppSettings _settings;

        public SettingsService()
        {
            _settingsPath = Path.Combine(FileSystem.AppDataDirectory, "settings.json");
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json);
                }
                _settings ??= new AppSettings();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                _settings = new AppSettings();
            }
        }

        public void SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public string GetDefaultDownloadPath()
        {
            return _settings.DefaultDownloadPath;
        }

        public void SetDefaultDownloadPath(string path)
        {
            _settings.DefaultDownloadPath = path;
            SaveSettings();
        }
    }
}