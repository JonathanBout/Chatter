using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatter.Core.Parameters
{
	public class LoginParameters
	{
		public required string Username { get; set; }
		public required string Password { get; set; }
	}
}
