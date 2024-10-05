using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatter.Core.Results
{
	public class UserResult
	{
		public required string Username { get; set; }
		public byte[]? PublicKey { get; set; }
	}
}
