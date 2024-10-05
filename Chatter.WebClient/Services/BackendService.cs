using Chatter.Core.Parameters;
using Chatter.Core.Results;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace Chatter.WebClient.Services
{
	public class BackendService
	{
		const string AUTH_BASE = "api/auth";

		const string CHAT_BASE = "api/chat";

		private readonly HttpClient _httpClient = new HttpClient();

		public async Task<LoginResult?> Login(string username, string password)
		{
			var response = await _httpClient.PostAsJsonAsync(AUTH_BASE + "/login", new LoginParameters
			{
				Username = username,
				Password = password
			});

			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadFromJsonAsync<LoginResult>();
			}

			return null;
		}

		public Task<LoginResult?> Login(string username, byte[] privateKey) => throw new NotImplementedException();

		public async Task<bool> UploadPublicKey(byte[] publicKey)
		{
			var response = await _httpClient.PostAsJsonAsync(AUTH_BASE + "/set-public-key", new SetPublicKeyParameters
			{
				PublicKey = Convert.ToBase64String(publicKey)
			});

			return response.IsSuccessStatusCode;
		}

		public async Task<bool> SendMessage(string message, string recipient, bool isEncrypted = false)
		{
			var response = await _httpClient.PostAsJsonAsync(CHAT_BASE + "/send-message", new SendMessageParameters
			{
				Content = message,
				ReceiverName = recipient,
				IsEncrypted = isEncrypted
			});

			return response.IsSuccessStatusCode;
		}
	}
}
