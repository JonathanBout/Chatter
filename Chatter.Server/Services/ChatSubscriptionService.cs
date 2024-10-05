using Chatter.Server.Data;

namespace Chatter.Server.Services
{
	public class ChatSubscriptionService : IChatSubscriptionService
	{
		private event Func<Message, Task>? OnMessageReceived = null;

		public IDisposable Subscribe(User user, Func<Message, Task> onReceived)
		{
			return new MessageReceivedSubscription(this, user, onReceived);
		}

		public async Task Invoke(Message message)
		{
			if (OnMessageReceived is not null)
				await OnMessageReceived.Invoke(message);
		}

		private class MessageReceivedSubscription : IDisposable
		{
			private readonly Func<Message, Task> _onReceived;
			private readonly ChatSubscriptionService _service;
			private readonly Guid _userId;

			public MessageReceivedSubscription(ChatSubscriptionService service, User user, Func<Message, Task> onReceived)
			{
				_onReceived = onReceived;
				_userId = user.Id;
				_service = service;
				service.OnMessageReceived += OnMessageReceivedInternal;
			}

			private async Task OnMessageReceivedInternal(Message message)
			{
				if (message.Receiver.Id == _userId)
					await _onReceived.Invoke(message);
			}

			public void Dispose()
			{
				_service.OnMessageReceived -= OnMessageReceivedInternal;
			}
		}
	}
}
