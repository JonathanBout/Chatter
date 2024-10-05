using System.ComponentModel.DataAnnotations;

namespace Chatter.Server.Configuration
{
	public class PasswordHasherOptions
	{
		[Range(10, int.MaxValue)]
		public int WorkFactor { get; set; } = 12;
	}
}
