using Chatter.Core;
using Chatter.Core.Parameters;
using Chatter.Core.Results;
using Chatter.Server.Data;
using Chatter.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chatter.Server.Controllers
{
	[ApiController]
	[Route("/api/chat")]
	[Authorize]
	public class ChatController(IChatService chatService, IUserService userService) : ControllerBase
	{
		private readonly IChatService _chatService = chatService;
		private readonly IUserService _userService = userService;

		private User GetCurrentUser() => _userService.GetUserById(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!))!;

		[HttpPost("send-message")]
		public async Task<Results<Created, BadRequest>> SendMessage(SendMessageParameters parameters)
		{
			User? receiver;

			if (parameters.ReceiverId == Guid.Empty)
				receiver = _userService.GetUserByUsername(parameters.ReceiverName);
			else
				receiver = _userService.GetUserById(parameters.ReceiverId);

			if (receiver is null)
				return TypedResults.BadRequest();

			var message = new Message
			{
				Sender = GetCurrentUser(),
				Receiver = receiver,
				Content = parameters.Content,
				SentAt = DateTimeOffset.Now,
				IsEncrypted = parameters.IsEncrypted
			};

			await _chatService.SendMessageAsync(message);
			return TypedResults.Created();
		}

		/// <summary>
		/// Opens a real-time WebSocket connection
		/// </summary>
		/// <returns></returns>
		[HttpGet("real-time-receive")]
		public async Task<Results<EmptyHttpResult, BadRequest>> BeginConnection()
		{
			if (!HttpContext.WebSockets.IsWebSocketRequest)
				return TypedResults.BadRequest();

			using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

			var user = GetCurrentUser();

			using (await _chatService.RegisterOnMessageReceived(user, _handleNewMessage))
			{
				// wait until the connection isn't open anymore
				while (!webSocket.CloseStatus.HasValue && webSocket.State == WebSocketState.Open) ;
			}

			return TypedResults.Empty;

			async Task _handleNewMessage(Message newMessage)
			{
				await HandleNewMessage(webSocket, newMessage);
			}
		}

		private async Task HandleNewMessage(WebSocket webSocket, Message newMessage)
		{
			var result = new NewMessageResult
			{
				Content = newMessage.Content,
				IsEncrypted = newMessage.IsEncrypted,
				SentTime = newMessage.SentAt,
				Sender = new UserResult
				{
					Username = newMessage.Sender.Username,
					PublicKey = newMessage.Sender.PublicKey
				}
			};

			if (webSocket.CloseStatus.HasValue)
				return;

			await webSocket.SendAsync(
				Encoding.Unicode.GetBytes(JsonSerializer.Serialize(result)),
				WebSocketMessageType.Text,
				true,
				CancellationToken.None
			);

			await _chatService.MessageDelivered(newMessage);
		}
	}
}
