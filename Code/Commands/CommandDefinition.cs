using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public class CommandDefinition
{
	public static IReadOnlyList<CommandDefinition> All => all.AsReadOnly();
	private static List<CommandDefinition> all = new();
	private static Dictionary<string, CommandDefinition> keyToDefinition = new();
	static CommandDefinition()
	{
		LoadAll();
	}
	public static CommandDefinition Get(string key)
	{
		if ( keyToDefinition.TryGetValue( key, out CommandDefinition cmd ) )
			return cmd;

		return null;
	}
	public static void LoadAll()
	{
		all.Clear();
		keyToDefinition.Clear();

		foreach((MethodDescription method, CommandAttribute attrib) in TypeLibrary.GetMethodsWithAttribute<CommandAttribute>())
		{
			CommandDefinition definition = new();
			definition.Key = attrib.Key;

			DisplayInfo info = method.GetDisplayInfo();
			definition.Title = info.Name;
			definition.Description = info.Description;
			definition.Group = info.Group;

			for ( int i = 0; i < method.Parameters.Length; i++ )
			{
				var param = method.Parameters[i];
				CommandParameter paramDefinition = new( param );
				// Check if the previous parameter was optional but the next one isnt
				if(!paramDefinition.Optional && i > 0 && definition.Parameters[i-1].Optional )
				{
					Log.Warning( $"Invalid required parameter after optional! {paramDefinition}" );
					break;
				}

				definition.Parameters.Add( paramDefinition );
			}

			definition.Callback = ( object[] p ) =>
			{
				method.Invoke( null, p );
			};

			all.Add( definition );
			keyToDefinition.Add(definition.Key, definition);
		}
	}
	public string Key { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public string Group { get; set; }
	public List<CommandParameter> Parameters { get; set; } = new();
	public Action<object[]> Callback { get; set; }
	public void Run(params object[] args)
	{
		Assert.NotNull( args );
		Assert.NotNull( Callback );

		int requiredParameters = Parameters.Count( p => !p.Optional );
		if(args.Length < requiredParameters )
		{
			Log.Error( $"Command {Key} cannot be called, not enough arguments!" );
			return;
		}

		try
		{
			Callback.Invoke( args.Take( Parameters.Count ).ToArray() );
		}
		catch(Exception e)
		{
			Log.Error( $"Error while running command {Key}: {e.Message}" );
		}
	}
	public void Run(params string[] args)
	{
		Assert.NotNull( args );
		Assert.NotNull( Callback );

		int requiredParameters = Parameters.Count( p => !p.Optional );
		if ( args.Length < requiredParameters )
		{
			Log.Error( $"Command {Key} cannot be called, not enough arguments!" );
			return;
		}
		List<object> parsedArgs = new();
		for ( int i = 0; i < args.Length && i < Parameters.Count; i++ )
		{
			string arg = args[i];
			var p = Parameters[i];

			object parsed = p.Parse( arg );
			if(parsed == null)
			{
				Log.Error( $"Could not parse argument {arg} to type {p.Type}" );
				return;
			}
			parsedArgs.Add(parsed);
		}

		try
		{
			Callback.Invoke( parsedArgs.ToArray() );
		}
		catch ( Exception e )
		{
			Log.Error( $"Error while running command {Key}: {e.Message}" );
		}
	}
}

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
	public string Key { get; set; }
	public CommandAttribute(string key)
	{
		if ( string.IsNullOrEmpty( key ) )
			throw new ArgumentNullException( nameof( key ) );

		Key = key;
	}
}
