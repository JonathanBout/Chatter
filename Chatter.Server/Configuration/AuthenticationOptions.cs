using System.ComponentModel.DataAnnotations;

namespace Chatter.Server.Configuration
{
	public class AuthenticationOptions
	{
		[Required]
		public required string Secret { get; set; }
		[Required]
		public required string Issuer { get; set; }
		[Required]
		public required string Audience { get; set; }
	}
}
