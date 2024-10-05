using BCrypt.Net;
using Chatter.Server.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
using Crypt = BCrypt.Net.BCrypt;

namespace Chatter.Server.Services
{
	public class PasswordHasher(IOptionsSnapshot<PasswordHasherOptions> options) : ISecureHasher
	{
		private static readonly Encoding UsedEncoding = Encoding.Unicode;

		private readonly PasswordHasherOptions _options = options.Value;

		public byte[] HashPassword(string password)
		{
			return UsedEncoding.GetBytes(Crypt.EnhancedHashPassword(password, _options.WorkFactor));
		}

		public PasswordVerifyResult VerifyPassword(string password, byte[] passwordHash)
		{
			string storedHash = UsedEncoding.GetString(passwordHash);

			if (Crypt.EnhancedVerify(password, storedHash))
			{
				if (Crypt.PasswordNeedsRehash(storedHash, _options.WorkFactor))
				{
					return PasswordVerifyResult.ValidNeedsRehash;
				} else
				{
					return PasswordVerifyResult.Valid;
				}
			}

			return PasswordVerifyResult.Invalid;
		}
	}

	public enum PasswordVerifyResult
	{
		Valid,
		Invalid,
		ValidNeedsRehash
	}
}
