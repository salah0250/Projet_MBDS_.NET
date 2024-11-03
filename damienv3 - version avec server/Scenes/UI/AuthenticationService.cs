using Godot;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

public class AuthenticationService
{
	private static string? _currentUsername;
	public static string? CurrentUsername 
	{ 
		get => _currentUsername;
		private set => _currentUsername = value;
	}
	public static void UpdateCurrentUsername(string newUsername)
	{
		CurrentUsername = newUsername;
	}
	private static readonly HttpClientHandler handler = new HttpClientHandler()
	{
		CookieContainer = new System.Net.CookieContainer(),
		UseCookies = true
	};
	
	 private static readonly System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient(handler)
	{
		DefaultRequestHeaders = 
		{
			Accept = { new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json") }
		}
	};
	
	private const string BaseUrl = "http://localhost:5231";

	private class LoginRequest
	{
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}
	
	public static async Task<bool> LoginAsync(string email, string password)
	{
		try
		{
			var loginRequest = new LoginRequest
			{
				Email = email,
				Password = password
			};

			// Convertir l'objet en JSON string
			var jsonContent = JsonSerializer.Serialize(loginRequest);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

			// Log de la requête pour débogage
			GD.Print($"Sending request to: {BaseUrl}/Bearer/login?useCookies=true&useSessionCookies=true");
			GD.Print($"Request content: {jsonContent}");

			// Faire la requête de connexion
			var response = await httpClient.PostAsync(
				$"{BaseUrl}/Bearer/login?useCookies=true&useSessionCookies=true",
				content
			);

			// Log de la réponse pour débogage
			var responseContent = await response.Content.ReadAsStringAsync();
			GD.Print($"Response status: {response.StatusCode}");
			GD.Print($"Response content: {responseContent}");

			if (response.IsSuccessStatusCode)
			{
				CurrentUsername = email;
				
				// Sauvegarder les cookies
				var cookies = handler.CookieContainer.GetCookies(new Uri(BaseUrl));
				foreach (System.Net.Cookie cookie in cookies)
				{
					GD.Print($"Received cookie: {cookie.Name} = {cookie.Value}");
				}
				
				SaveCookies(cookies);
				
				// Sauvegarder la session
				SessionManager.Instance.SaveSession(email);
				
				return true;
			}
			
			GD.PrintErr($"Login failed with status: {response.StatusCode}");
			GD.PrintErr($"Response content: {responseContent}");
			
			return false;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Login error: {ex.Message}");
			if (ex.InnerException != null)
			{
				GD.PrintErr($"Inner exception: {ex.InnerException.Message}");
			}
			return false;
		}
	}

	private static void SaveCookies(System.Net.CookieCollection cookies)
	{
		var config = new ConfigFile();
		foreach (System.Net.Cookie cookie in cookies)
		{
			config.SetValue("Cookies", cookie.Name, new Godot.Collections.Dictionary<string, string>
			{
				{ "value", cookie.Value },
				{ "domain", cookie.Domain },
				{ "path", cookie.Path }
			});
		}
		config.Save("user://cookies.cfg");
	}

	private static void RestoreCookies()
	{
		var config = new ConfigFile();
		if (config.Load("user://cookies.cfg") == Error.Ok)
		{
			foreach (var cookieName in config.GetSectionKeys("Cookies"))
			{
				var cookieData = (Godot.Collections.Dictionary)config.GetValue("Cookies", cookieName);
				var cookie = new System.Net.Cookie(
					cookieName,
					cookieData["value"].ToString(),
					cookieData["path"].ToString(),
					cookieData["domain"].ToString()
				);
				handler.CookieContainer.Add(new Uri(BaseUrl), cookie);
			}
		}
	}

	public static void RestoreSession(string username)
	{
		CurrentUsername = username;
		RestoreCookies();
	}

	public static async Task<bool> ValidateTokenAsync()
	{
		try
		{
			var response = await httpClient.GetAsync($"{BaseUrl}/api/1.0.0/Users/GetUserProfile");
			return response.IsSuccessStatusCode;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Validation error: {ex.Message}");
			return false;
		}
	}

	public static void Logout()
	{
		CurrentUsername = null;
		handler.CookieContainer = new System.Net.CookieContainer();
		var dir = DirAccess.Open("user://");
		if (dir != null)
		{
			dir.Remove("cookies.cfg");
		}
	}
	public static string? GetCookieValue(string cookieName)
   {
	var cookies = handler.CookieContainer.GetCookies(new Uri(BaseUrl));
	var cookie = cookies[cookieName];
	return cookie?.Value;
	}
}
