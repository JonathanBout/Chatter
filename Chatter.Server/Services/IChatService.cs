using Chatter.Server.Data;
using System.Collections.ObjectModel;

namespace Chatter.Server.Services
{
	public interface IChatService
	{
		IReadOnlyList<Message> GetAvailableMessagesForUser(User user);
		Task MessageDelivered(Message message);
		Task<IDisposable> RegisterOnMessageReceived(User user, Func<Message, Task> onReceived);
		Task SendMessageAsync(Message message);
	}
}