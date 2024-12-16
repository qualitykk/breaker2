using System.Reflection;

namespace Breaker;

public static class Console
{
	internal static void CreateDefaultCommands()
	{
		CommandDefinition.Create( "help", nameof(Console), "Help" );
		CommandDefinition.Create( "module", nameof( Console ), "ModuleToggle" );
	}
	[ConCmd("brk")]
	public static void RunCommand(string command = "", string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "")
	{
		if(!Game.InGame || Game.ActiveScene == null || Game.ActiveScene.IsEditor)
		{
			Log.Warning( "Cant run breaker commands outside of the game." );
			return;
		}

		if(!AdminSystem.Active)
		{
			Log.Warning( "Breaker has not been loaded yet. Start scene or use brk_reload to load." );
			return;
		}

		if(string.IsNullOrEmpty(command))
		{
			CommandDefinition.Run( "help" );
			return;
		}

		var parameters = new string[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 };

		CommandDefinition.Run( command, parameters );
	}

	public static void Help(string search = "")
	{
		var user = CommandContext.Caller;
		List<CommandDefinition> cmds = CommandDefinition.All.ToList().FindAll(def => def.Key.Contains(search, StringComparison.OrdinalIgnoreCase));
		if(user != default)
		{
			cmds = cmds.FindAll( def =>user.HasPermission( def.Permissions))
						.OrderBy( def => def.Key ).ToList();
		}
		else
		{
			cmds = cmds.OrderBy(def => def.Key).ToList();
		}

		Message.Caller( "============" );
		Message.Caller( $"{Globals.NAME} ({Globals.VERSION}/{Globals.BRANCH})" );
		Message.Caller( "============" );

		int length = cmds.Count;
		if(length == 0)
		{
			Message.Caller( $"No commands with id {search} found." );
		}
		else if(length == 1) 
		{
			PrintCommand( cmds.Single() );
		}
		else
		{
			Message.Caller( $"{length} command found." );
			foreach ( var cmd in cmds )
			{
				PrintCommand( cmd );
			}
		}
		
	}
	/// <summary>
	/// Manages command modules.
	/// Valid actions for modules: enable, disable, toggle, save.
	/// </summary>
	public static void ModuleToggle( [Title( "enable/disable/toggle/save" )] string action = "", string name = "" )
	{
		switch(action)
		{
			case "enable":
				Message.Caller( $"Enabled module {name}" );
				AdminSystem.EnableModule( name );
				break;
			case "disable":
				AdminSystem.DisableModule( name );
				Message.Caller( $"Disabled module {name}" );
				break;
			case "toggle":
				var module = AdminSystem.FindModule( name );
				if ( module.Enabled )
				{
					AdminSystem.DisableModule( module );
					Message.Caller( $"Disabled module {name}" );
				}
				else
				{
					AdminSystem.EnableModule( module );
					Message.Caller( $"Enabled module {name}" );
				}
				break;
			case "save":
				AdminSystem.SaveModules();
				Message.Caller( "Saved disabled modules to config." );
				break;
			default:
				PrintModules();
				break;
		}
	}
	private static void PrintCommand( CommandDefinition cmd )
	{
		string name = cmd.Key;
		var p = cmd.Parameters;
		if ( p.Length > 0 )
		{
			name += $" {string.Join( " ", p.Select( x => $"<{x.Name}: {PrettyTypeName( x.GetLibraryType() )}>" ) )}";
		}

		Message.Caller( $"- {name}" );
		string desc = cmd.Description;
		if ( !string.IsNullOrEmpty( desc ) )
		{
			foreach(var line in desc.Split( '\n' ) )
			{
				Message.Caller( $"=> {line}" );
			}
		}
	}
	private static string PrettyTypeName( TypeDescription desc )
	{
		var t = desc.TargetType;
		if ( t == typeof( string ) )
			return "String";
		if ( t == typeof( int ) )
			return "Integer";
		if ( t == typeof( float ) )
			return "Float";
		if ( t == typeof( bool ) )
			return "Boolean";
		if ( t == typeof( long ) )
			return "Long";
		if ( t == typeof( Vector3 ) )
			return "Vector";
		if ( t == typeof( TargetUser ) )
			return "User(s)";
		return t.Name;
	}
	private static void PrintModules()
	{
		var modules = AdminSystem.Instance.Modules;
		var loaded = modules.Where( m => m.Enabled );
		var unloaded = modules.Where( m => !m.Enabled );

		Message.Caller( "============" );
		Message.Caller( $"{Globals.NAME} ({Globals.VERSION}/{Globals.BRANCH})" );
		Message.Caller( "============" );
		Message.Caller( $"{modules.Count} modules enabled" );

		if(loaded.Any())
		{
			Message.Caller( "Enabled Modules:" );
			foreach ( var m in loaded )
			{
				Message.Caller( $"- {DisplayInfo.For( m ).Name}" );
			}
		}

		if( unloaded.Any())
		{
			Message.Caller( "Disabled Modules:" );
			foreach ( var m in unloaded )
			{
				Message.Caller( $"- {DisplayInfo.For( m ).Name}" );
			}
		}
	}
}
