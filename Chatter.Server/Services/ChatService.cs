using Chatter.Server.Data;
using System.Collections.ObjectModel;

namespace Chatter.Server.Services
{
	public class ChatService(ChatDatabaseContext database, IChatSubscriptionService subscriptionManager) : IChatService
	{
		private readonly ChatDatabaseContext _database = database;
		private readonly IChatSubscriptionService _subscriptionManager = subscriptionManager;

		public async Task SendMessageAsync(Message message)
		{
			_database.Add(message);
			_database.SaveChanges();
			await _subscriptionManager.Invoke(message);
		}

		public IReadOnlyList<Message> GetAvailableMessagesForUser(User user)
		{
			return GetAvailableMessagesForUser(user, default);
		}

		public IReadOnlyList<Message> GetAvailableMessagesForUser(User user, Guid lastReceivedMessageId = default)
		{
			var entry = _database.Entry(user);
			entry.Reload();

			user = entry.Entity;

			var lastReceivedMessage = user.AvailableMessages.FirstOrDefault(m => m.Id == lastReceivedMessageId);

			var messages = new List<Message>();
			
			if (lastReceivedMessage is null)
			{
				return [..user.AvailableMessages];
			}

			return user.AvailableMessages.Where(m => m.SentAt > lastReceivedMessage.SentAt).ToList();
		}

		public async Task<IDisposable> RegisterOnMessageReceived(User user, Func<Message, Task> onReceived)
		{
			// Send all available messages to the user
			var messages = GetAvailableMessagesForUser(user);

			foreach (var message in messages)
				await onReceived.Invoke(message);

			// return a subscription
			return _subscriptionManager.Subscribe(user, onReceived);
		}

		public Task MessageDelivered(Message message)
		{
			var reloaded = _database.Messages.Find(message.Id)!;

			reloaded.DeliveredAt = DateTimeOffset.Now;

			_database.SaveChanges();

			return Task.CompletedTask;
		}
	}
}
