using Chatter.Server.Data;
using System.IdentityModel.Tokens.Jwt;

namespace Chatter.Server.Services
{
	public interface IUserService
	{
		void AddUser(User user);
		void UpdatePublicKey(User user, byte[] publicKey);
		User? GetUserById(Guid id);
		User? GetUserByUsername(string username);
		IEnumerable<User> SearchUsers(string query, byte pageSize, uint page);
		Task<JwtSecurityToken?> AuthorizeUser(string username, string password);
		Task<JwtSecurityToken?> AuthorizeUser(string username, byte[] signature);
	}
}