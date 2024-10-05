using Chatter.Core;
using Chatter.Core.Parameters;
using Chatter.Core.Results;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace Chatter.TerminalClient
{
	public sealed class Client(string serverName)
	{
		private readonly string _serverName = serverName;

		private readonly ClientWebSocket _socket = new();

		private readonly HttpClient _httpClient = new()
		{
			BaseAddress = new Uri($"https://{serverName}/")
		};

		private CancellationTokenSource _closingCts = new();

		private readonly Queue<NewMessageResult> _receivedMessagesQueue = [];

		private string? AuthToken { get; set; } = null;

		private byte[]? PrivateKey { get; set; } = null;

		private async Task<bool> Login()
		{

			string username = Helpers.GetStringInput("username:");

			PrivateKey = LoadPrivateKey(username);

			LoginResult result;

			// if no private key has been generated yet, we fall back to password auth

			// Admin password is 51239656D383
			// DefaultUser password is A30CD230F32E

			if (PrivateKey is null)
			{
				string password = Helpers.GetStringInput("username:");

				var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new LoginParameters
				{
					Username = username,
					Password = password
				});

				if (!response.IsSuccessStatusCode
					|| await response.Content.ReadFromJsonAsync<LoginResult>() is not LoginResult loginResult)
				{
					return false;
				}

				result = loginResult;
			} else
			{
				using var rsa = RSA.Create();

				rsa.ImportRSAPrivateKey(PrivateKey, out _);

				byte[] signature = rsa.SignData(Encoding.Unicode.GetBytes(username), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

				var response = await _httpClient.PostAsJsonAsync("/api/auth/login?signature=true", new LoginParameters
				{
					Username = username,
					Password = Convert.ToBase64String(signature)
				});

				if (!response.IsSuccessStatusCode
					|| await response.Content.ReadFromJsonAsync<LoginResult>() is not LoginResult loginResult)
				{
					return false;
				}

				result = loginResult;
			}

			AuthToken = result.Token;

			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);

			if (PrivateKey is null)
			{
				(var publicKey, PrivateKey) = GenerateKeyPair(username);

				(await _httpClient.PostAsJsonAsync("/api/auth/set-public-key", new SetPublicKeyParameters(publicKey))).EnsureSuccessStatusCode();
			}

			return true;
		}

		private async Task Connect()
		{
			_closingCts.Dispose();
			_closingCts = new CancellationTokenSource();

			if (_socket.CloseStatus is not null)
				await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection was not open anymore", CancellationToken.None);

			_socket.Options.SetRequestHeader("Authorization", $"Bearer {AuthToken}");

			await _socket.ConnectAsync(new Uri($"wss://{_serverName}/api/chat/real-time-receive"), CancellationToken.None);
		}

		private async Task Disconnect()
		{
			await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client requested closure", CancellationToken.None);
			_closingCts.Cancel();
			_socket.Options.SetRequestHeader("Authorization", null);
			_httpClient.DefaultRequestHeaders.Authorization = null;
		}

		public async Task Launch()
		{
			while (!await Login())
			{
				Console.WriteLine("Invalid credentials. Please try again.");
			}

			await Connect();
			// run the receive loop in the background
			_ = Task.Run(ReceiveLoop);

			while (!_closingCts.IsCancellationRequested)
			{
				//var receiverGuid = Helpers.GetGuidInput("Please enter your receiver's GUID:");

				var receiver = await SelectUser();

				while (!_closingCts.IsCancellationRequested)
				{
					var message = Helpers.GetStringInput("Please enter your message (type '_back' to go back, type '_receive' to show new messages, type '_close' to close the connection):");

					if (message == "_back")
					{
						break;
					}

					if (message == "_receive")
					{
						if (_socket.State != WebSocketState.Open || _socket.CloseStatus is not null)
						{
							await Connect();
						}

						while (_receivedMessagesQueue.TryDequeue(out NewMessageResult? result))
						{
							Console.WriteLine("From {0} at {1}: {2}", result.Sender.Username, result.SentTime, result.Content);
						}
						continue;
					}

					if (message == "_close")
					{
						await Disconnect();

						return;
					}

					await SendMessage(receiver, message);
				}
			}

			await Disconnect();
		}

		private async Task ReceiveLoop()
		{
			while (_socket.State == WebSocketState.Open && !_closingCts.IsCancellationRequested)
			{
				using var rent = ArrayPool<byte>.Shared.RentDisposable(1024);
				var result = await _socket.ReceiveAsync(rent.Segment, CancellationToken.None);

				if (result.MessageType == WebSocketMessageType.Close)
				{
					return;
				}

				var messageJSON = Encoding.Unicode.GetString(rent.Span[..result.Count]);

				var message = JsonSerializer.Deserialize<NewMessageResult>(messageJSON)!;

				if (message.IsEncrypted)
				{
					using var rsa = RSA.Create();

					rsa.ImportRSAPrivateKey(PrivateKey, out _);

					var messageBytes = Convert.FromBase64String(message.Content);

					string decryptedMessage = Encoding.Unicode.GetString(rsa.Decrypt(messageBytes, RSAEncryptionPadding.OaepSHA512));

					message.IsEncrypted = false;
					message.Content = decryptedMessage;
				}

				if (message is not null)
				{
					_receivedMessagesQueue.Enqueue(message);
				}
			}
		}

		private async Task<UserResult> SelectUser()
		{
			var query = Helpers.GetStringInput("Search for a user:");
			query = HttpUtility.UrlEncode(query);
			var response = await _httpClient.GetFromJsonAsync<List<UserResult>>($"/api/users/search?query={query}") ?? [];

			int i = 0;

			for (; i < response.Count; i++)
			{
				UserResult? user = response[i];
				Console.WriteLine("{0}. {1}", i, user.Username);
			}

			var userFound = Helpers.GetBoolInput("Do you want to send a message to any of these users?");

			if (!userFound)
			{
				return await SelectUser();
			}

			var userIndex = Helpers.GetIntInput("Please enter the number of the user you want to send a message to:", 0, i);

			return response[userIndex];
		}

		private async Task SendMessage(UserResult receiver, string message)
		{
			bool encrypted = false;

			if (receiver.PublicKey is not null)
			{
				using var rsa = RSA.Create();
				rsa.ImportRSAPublicKey(receiver.PublicKey, out _);

				message = Convert.ToBase64String(rsa.Encrypt(Encoding.Unicode.GetBytes(message), RSAEncryptionPadding.OaepSHA512));
				encrypted = true;
			}

			var response = await _httpClient.PostAsJsonAsync("/api/chat/send-message", new SendMessageParameters(receiver.Username, message, encrypted));
			response.EnsureSuccessStatusCode();
		}

		private static byte[]? LoadPrivateKey(string username)
		{
			var keyPath = GetPrivateKeyPath(username);

			return File.ReadAllBytes(keyPath);
		}

		private static (byte[] publicKey, byte[] privateKey) GenerateKeyPair(string username)
		{
			using var rsa = RSA.Create();
			var publicKey = rsa.ExportRSAPublicKey();
			var privateKey = rsa.ExportRSAPrivateKey();

			var keyPath = GetPrivateKeyPath(username);
			File.WriteAllBytes(keyPath, privateKey);
			return (publicKey, privateKey);
		}

		private static string GetPrivateKeyPath(string username)
		{
			username = string.Join("_", username.Split([..Path.GetInvalidFileNameChars(), '/'], StringSplitOptions.RemoveEmptyEntries));


			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chatter", username);

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return Path.Combine(path, "private.key");
		}

		~Client()
		{
			_closingCts.Dispose();
			_socket.Dispose();
			_httpClient.Dispose();
		}
	}
}
