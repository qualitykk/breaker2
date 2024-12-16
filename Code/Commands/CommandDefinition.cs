using Sandbox.Diagnostics;

namespace Breaker;

public partial class CommandDefinition
{
	public static IReadOnlyList<CommandDefinition> All => all.AsReadOnly();
	private static List<CommandDefinition> all { get; set; } = new();
	private static Dictionary<string, CommandDefinition> keyToDefinition { get; set; } = new();
	public static CommandDefinition Get(string key)
	{
		if ( keyToDefinition.TryGetValue( key, out CommandDefinition cmd ) )
			return cmd;

		return default;
	}
	/// <summary>
	/// Add method as command
	/// </summary>
	[Authority]
	public static void Create(string key, string typeName, string methodName, object instance = null)
	{
		Assert.NotNull( key );
		Assert.NotNull( typeName );
		Assert.NotNull( methodName );

		var type = TypeLibrary.GetType( typeName );
		if(type == null)
		{
			Logging.Error( $"Tried to add command method with invalid type {typeName}" );
			return;
		}

		var method = type.Methods.FirstOrDefault(type => type.Name == methodName );
		if(method == null)
		{
			Logging.Error( $"Tried to add command with non-existing method {methodName}" );
			return;
		}

		if (keyToDefinition.ContainsKey(key))
		{
			Logging.Error( $"Tried to add command with duplicate key {key}" );
			return;
		}
		if(!method.IsStatic && instance == null)
		{
			Logging.Message( "Tried to add non-static command without instance, ignoring", LogLevel.Warn );
			return;
		}

		CommandDefinition definition = new();
		definition.Key = key;

		DisplayInfo info = method.GetDisplayInfo();
		definition.Title = info.Name;
		definition.Description = info.Description;
		definition.Group = info.Group ?? method.TypeDescription.Title;

		var permissions = method.Attributes.OfType<PermissionAttribute>()
											.Where( attr => !attr.ManualEnforcement )
											.Select( attr => attr.Permission );
		definition.Permissions = permissions.ToArray();

		var parameters = new List<CommandParameter>();
		for ( int i = 0; i < method.Parameters.Length; i++ )
		{
			var param = method.Parameters[i];
			CommandParameter paramDefinition = new( param );
			// Check if the previous parameter was optional but the next one isnt
			if ( !paramDefinition.Optional && i > 0 && parameters[i - 1].Optional )
			{
				Logging.Message( $"Command {definition.Key} has invalid optional parameters!", LogLevel.Warn );
				break;
			}

			parameters.Add( paramDefinition );
		}

		definition.Parameters = parameters.ToArray();
		definition.Callback = ( object[] p ) =>
		{
			method.Invoke( instance, p );
		};

		all.Add( definition );
		keyToDefinition.Add( definition.Key, definition );
		Logging.Message( $"Added command {definition.Key}", LogLevel.Debug );

		return;
	}
	[Authority]
	public static void Remove(string key)
	{
		if(!keyToDefinition.TryGetValue(key, out var cmd))
		{
			Logging.Error( $"Tried to remove command but none with key {key} exists!" );
			return;
		}

		keyToDefinition.Remove( key );
		all.Remove( cmd );
	}
	internal static void Clear()
	{
		all.Clear();
		keyToDefinition.Clear();
	}

	public string Key { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public string Group { get; set; }
	/// <summary>
	/// User has to have all of these permission to execute this command.
	/// </summary>
	public string[] Permissions { get; set; }
	public CommandParameter[] Parameters { get; set; }
	public Action<object[]> Callback { get; set; }
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
