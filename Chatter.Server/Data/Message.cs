namespace Chatter.Server.Data
{
	public class Message
	{
		public Guid Id { get; set; }
		public virtual required User Sender { get; set; }
		public virtual required User Receiver { get; set; }
		public string Content { get; set; } = string.Empty;
		public DateTimeOffset SentAt { get; set; } = DateTimeOffset.Now;
		public DateTimeOffset DeliveredAt { get; set; } = default;
		public bool IsEncrypted { get; set; } = false;
	}
}
