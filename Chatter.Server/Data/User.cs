namespace Chatter.Server.Data
{
	public class User
	{
		public Guid Id { get; set; }
		public string Username { get; set; } = string.Empty;
		public byte[]? PublicKey { get; set; } = null;
		public byte[] PasswordHash { get; set; } = [];
		public virtual IList<Message> AvailableMessages { get; set; } = [];
	}
}
