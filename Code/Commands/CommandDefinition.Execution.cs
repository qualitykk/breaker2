using Sandbox.Diagnostics;

namespace Breaker;

public partial class CommandDefinition
{
	[Authority]
	public static void Run(string key, params string[] args)
	{
		Connection caller = Rpc.Caller;
		using var _ = CreateContext(caller);

		if(!keyToDefinition.TryGetValue( key, out var cmd ) )
		{
			Message.Caller($"Command {key} does not exist.");
			return;
		}

		if ( !CommandContext.Caller.HasPermission( cmd.Permissions ) )
		{
			Message.Caller( $"You do not have permission to run this command! ({string.Join( ',', cmd.Permissions )})", MessageType.Error );
			return;
		}

		int requiredParameters = cmd.Parameters.Count( p => !p.Optional );
		if ( args.Length < requiredParameters )
		{
			Logging.Error( $"Command {key} cannot be called, not enough arguments!" );
			return;
		}
		List<object> parsedArgs = new();
		int argCount = args?.Length ?? 0;
		for ( int i = 0; i < cmd.Parameters.Length; i++ )
		{
			var p = cmd.Parameters[i];
			if( i > argCount )
			{
				parsedArgs.Add( p.DefaultValue );
				continue;
			}

			string arg = args[i];

			object parsed = p.Parse( arg );
			if ( parsed == null )
			{
				if ( p.Optional )
				{
					parsed = p.DefaultValue;
				}
				else
				{
					Message.Caller( $"Cant run command {key}, missing required parameter at position {i + 1}! ({p.Name})" );
					return;
				}
			}
			else if ( parsed.GetType() != p.GetLibraryType().TargetType )
			{
				Log.Info( $"Parsed type doesnt match! {parsed} ({parsed.GetType()}) vs {p.GetLibraryType().TargetType}" );
			}
			parsedArgs.Add( parsed );
		}

		if( argCount > 0)
		{
			var targetParameters = args.OfType<TargetUser>();
			if ( targetParameters.Any() )
			{
				foreach ( var target in targetParameters )
				{
					for ( int i = 0; i < target.Count; i++ )
					{
						var user = target[i];
						if ( !CommandContext.Caller.CanTarget( user ) )
						{
							target.Targets.Remove( user );
						}
					}
				}
			}
		}
		
		Log.Info( $"Calling cmd with args: {string.Join( ';', parsedArgs )}" );
		cmd.Callback?.Invoke( parsedArgs.ToArray() );
	}

	private static IDisposable CreateContext(Connection conn)
	{
		return CommandContext.Push( new()
		{
			User = User.Get( conn.SteamId )
		} );
	}
}
