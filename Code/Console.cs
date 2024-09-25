namespace Breaker;

public static class Console
{
	[ConCmd("brk")]
	public static void RunCommand(string command, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "")
	{
		var parameters = new string[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 };

		var cmd = CommandDefinition.Get( command );
		if(cmd == null)
		{
			Log.Warning( $"No command with name {command} exists!" );
			return;
		}

		cmd.Run( parameters );
	}
}
