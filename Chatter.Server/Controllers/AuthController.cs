using Chatter.Core.Parameters;
using Chatter.Core.Results;
using Chatter.Server.Configuration;
using Chatter.Server.Data;
using Chatter.Server.Services;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Chatter.Server.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthController(IUserService userService) : ControllerBase
	{
		private readonly IUserService _userService = userService;

		private User? GetCurrentUser() => _userService.GetUserById(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));

		[HttpPost("login")]
		public async Task<Results<Ok<LoginResult>, UnauthorizedHttpResult>> Login(LoginParameters parameters, [FromQuery] bool signature = false)
		{
			JwtSecurityToken? token;

			if (signature)
			{
				token = await _userService.AuthorizeUser(parameters.Username, Convert.FromBase64String(parameters.Password));
			} else
			{
				token = await _userService.AuthorizeUser(parameters.Username, parameters.Password);
			}

			if (token is null)
			{
				return TypedResults.Unauthorized();
			}

			return TypedResults.Ok(new LoginResult { Token = new JwtSecurityTokenHandler().WriteToken(token) });
		}

		[HttpPost("set-public-key")]
		[Authorize]
		public Results<Ok, BadRequest> SetPublicKey(SetPublicKeyParameters parameters)
		{
			var user = GetCurrentUser()!;

			_userService.UpdatePublicKey(user, Convert.FromBase64String(parameters.PublicKey));

			return TypedResults.Ok();
		}
	}
}
