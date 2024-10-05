using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatter.TerminalClient
{
	public class Helpers
	{
		public static string GetStringInput(string message, int minLength = 0, int maxLength = 0)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(minLength);
			ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(minLength, maxLength);

			while (true)
			{
				Console.WriteLine(message);

				var input = Console.ReadLine() ?? "";

				if (minLength > 0 && input.Length < minLength)
				{
					Console.WriteLine($"Input must be at least {minLength} characters long.");
					continue;
				}

				if (maxLength > 0 && input.Length > maxLength)
				{
					Console.WriteLine($"Input must be at most {maxLength} characters long.");
					continue;
				}

				return input;
			}
		}

		public static Guid GetGuidInput(string message)
		{
			while (true)
			{
				var input = GetStringInput(message);

				if (Guid.TryParse(input, out Guid guid))
				{
					return guid;
				} else
				{
					Console.WriteLine("Input must be a valid GUID. (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)");
				}
			}
		}

		public static bool GetBoolInput(string message)
		{
			while (true)
			{
				var input = GetStringInput(message).ToLower();
				if (input is "y" or "yes")
				{
					return true;
				}
				if (input is "n" or "no")
				{
					return false;
				}
				Console.WriteLine("Input must be 'yes' or 'no'.");
			}
		}

		public static int GetIntInput(string message, int min = int.MinValue, int max = int.MaxValue)
		{
			while (true)
			{
				var input = GetStringInput(message);
				if (int.TryParse(input, out int number))
				{
					if (number >= min && number <= max)
					{
						return number;
					}
					Console.WriteLine($"Input must be between {min} and {max}.");
				} else
				{
					Console.WriteLine("Input must be a valid integer.");
				}
			}
		}
	}
}
