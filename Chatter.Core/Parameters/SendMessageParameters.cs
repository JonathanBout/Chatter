using System.ComponentModel.DataAnnotations;

namespace Chatter.Core.Parameters
{
	public class SendMessageParameters()
	{
		public Guid ReceiverId { get; set; } = Guid.Empty;
		public string ReceiverName { get; set; } = "";
		public string Content { get; set; } = "";
		public bool IsEncrypted { get; set; }

		public SendMessageParameters(Guid receiverId, string content, bool isEncrypted = false) : this()
		{
			ReceiverId = receiverId;
			Content = content;
			IsEncrypted = isEncrypted;
		}

		public SendMessageParameters(string receiverName, string content, bool isEncrypted = false) : this()
		{
			ReceiverName = receiverName;
			Content = content;
			IsEncrypted = isEncrypted;
		}
	}
}