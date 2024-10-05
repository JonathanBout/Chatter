using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatter.Core.Parameters
{
	public class SetPublicKeyParameters()
	{
		[Required]
		public string PublicKey { get; set; } = "";

		public SetPublicKeyParameters(byte[] publicKey) : this()
		{
			PublicKey = Convert.ToBase64String(publicKey);
		}
	}
}
