using Chatter.TerminalClient;

Console.WriteLine("Welcome to Chatter!");

Console.WriteLine();

string serverName = Helpers.GetStringInput("Please enter the name of the server you will be connecting to:");

Console.WriteLine("Connecting to {0}...", serverName);

do
{
	Console.Clear();
	try
	{
		var client = new Client(serverName);
		await client.Launch();
	} catch (Exception ex)
	{
		Console.WriteLine("An error occurred: {0}", ex.Message);

		if (Helpers.GetBoolInput("Show advanced info?"))
		{
			Console.WriteLine(ex.StackTrace);
			Console.WriteLine();
		}
	}
} while (!Helpers.GetBoolInput("Do you want to quit?"));
