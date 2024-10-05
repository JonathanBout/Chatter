namespace Chatter.Server.Services
{
	public interface ISecureHasher
	{
		byte[] HashPassword(string password);
		PasswordVerifyResult VerifyPassword(string password, byte[] passwordHash);
	}
}