using Chatter.Server.Configuration;
using Chatter.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Chatter.Server.Services
{
	public class UserService(ChatDatabaseContext database, ISecureHasher hasher, IOptionsSnapshot<AuthenticationOptions> authOptions) : IUserService
	{
		private readonly ChatDatabaseContext _database = database;
		private readonly ISecureHasher _hasher = hasher;
		private readonly AuthenticationOptions _authConfig = authOptions.Value;

		public void AddUser(User user)
		{
			_database.Users.Add(user);
		}

		public User? GetUserById(Guid id)
		{
			return _compiledUserByIdQuery(_database, id);
		}

		public User? GetUserByUsername(string username)
		{
			return _compiledUserByUsernameQuery(_database, username);
		}

		public Task<JwtSecurityToken?> AuthorizeUser(string username, string password)
		{
			var user = GetUserByUsername(username);

			if (user is null)
				return Task.FromResult<JwtSecurityToken?>(null);

			switch (_hasher.VerifyPassword(password, user.PasswordHash))
			{
				case PasswordVerifyResult.Invalid:
					return Task.FromResult<JwtSecurityToken?>(null);
				case PasswordVerifyResult.ValidNeedsRehash:

					var newHash = _hasher.HashPassword(password);
					user.PasswordHash = newHash;
					_database.SaveChanges();

					return AuthorizeUser(username, password);
				default:
					var token = GenerateAccessToken(user);
					return Task.FromResult(token)!;
			}
		}

		public Task<JwtSecurityToken?> AuthorizeUser(string username, byte[] signature)
		{
			var user = GetUserByUsername(username);

			if (user is null)
				return Task.FromResult<JwtSecurityToken?>(null);

			using var rsa = RSA.Create();
			rsa.ImportRSAPublicKey(user.PublicKey, out _);

			if (rsa.VerifyData(Encoding.Unicode.GetBytes(user.Username), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
			{
				var token = GenerateAccessToken(user);
				return Task.FromResult(token)!;
			}

			return Task.FromResult<JwtSecurityToken?>(null);
		}

		public void UpdatePublicKey(User user, byte[] key)
		{
			var entry = _database.Entry(user);
			entry.Reload();
			user = entry.Entity;

			user.PublicKey = key;

			_database.SaveChanges();
		}

		public IEnumerable<User> SearchUsers(string query, byte pageSize, uint page)
		{
			return _compiledUserSearchQuery(_database, query.Split(' '))
				.Skip((int)page * pageSize)
				.Take(pageSize);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EF doesn't know the overloaded method")]
		private static readonly Func<ChatDatabaseContext, IEnumerable<string>, IEnumerable<User>> _compiledUserSearchQuery
			= EF.CompileQuery((ChatDatabaseContext database, IEnumerable<string> query) => database.Users.Where(user => query.All(kw => user.Username.ToLower().Contains(kw.ToLower()))));

		private static readonly Func<ChatDatabaseContext, Guid, User?> _compiledUserByIdQuery
			= EF.CompileQuery((ChatDatabaseContext database, Guid id) => database.Users.FirstOrDefault(user => user.Id == id));
		//= (ChatDatabaseContext database, Guid id) => database.Users.Find(id);

		private static readonly Func<ChatDatabaseContext, string, User?> _compiledUserByUsernameQuery
			= EF.CompileQuery((ChatDatabaseContext database, string username) => database.Users.FirstOrDefault(user => user.Username == username));

		private JwtSecurityToken GenerateAccessToken(User user)
		{
			// Create user claims
			var claims = new List<Claim>
			{
				new(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new(ClaimTypes.Name, user.Username)
			};

			// Create a JWT
			var token = new JwtSecurityToken(
				issuer: _authConfig.Issuer,
				audience: _authConfig.Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddHours(1), // Token expiration time
				signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.Unicode.GetBytes(_authConfig.Secret)),
					SecurityAlgorithms.HmacSha256)
			);

			return token;
		}

	}
}
