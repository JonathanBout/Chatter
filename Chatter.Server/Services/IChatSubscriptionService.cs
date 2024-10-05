using Chatter.Server.Data;

namespace Chatter.Server.Services
{
	public interface IChatSubscriptionService
	{
		Task Invoke(Message message);
		IDisposable Subscribe(User user, Func<Message, Task> onReceived);
	}
}