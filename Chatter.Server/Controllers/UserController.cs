using Chatter.Core.Results;
using Chatter.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Chatter.Server.Controllers
{
	[Route("api/users")]
	[ApiController]
	public class UserController(IUserService userService) : ControllerBase
	{
		private readonly IUserService _userService = userService;

		[HttpGet("search")]
		public Ok<List<UserResult>> SearchUsers([FromQuery] string query, byte pageSize = 10, uint page = 0)
		{
			var users = _userService.SearchUsers(query, pageSize, page);
			var result = users.Select(u => new UserResult { Username = u.Username, PublicKey = u.PublicKey }).ToList();
			return TypedResults.Ok(result);
		}
	}
}
