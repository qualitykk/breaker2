using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public class AdminSystem : GameObjectSystem, Component.INetworkListener
{
	[HostSync] public static bool Active { get; set; }
	public static AdminSystem Instance { get; set; }
	public IReadOnlyList<CommandModule> Modules => modules.AsReadOnly();
	[HostSync, SkipHotload] private static NetList<CommandModule> modules { get; set; } = new();
	private bool hostAdded = false;
	public AdminSystem( Scene scene ) : base( scene )
	{
		if ( !Networking.IsHost ) return;
		Reload();
		Instance = this;

		Listen( Stage.FinishFixedUpdate, 1, CheckNetworkingActive, "CheckNetworkingActive" );
	}

	private void CheckNetworkingActive()
	{
		if ( Networking.IsActive )
		{
			if( !hostAdded )
			{
				InitConnection( Connection.Host );
				hostAdded = true;
			}
			Active = true;
		}
		else
		{
			Active = false;
			hostAdded = false;
		}
	}

	/// <summary>
	/// Reloads config files, commands and user data.
	/// </summary>
	[ConCmd("brk_reload")]
	[Authority(NetPermission.HostOnly)]
	public static void Reload()
	{
		CommandDefinition.Clear();
		AdminEvent.Reset();
		Logging.ClearLoggers();

		Logging.Info( "Loading admin system..." );
		Database.Init();
		Config.Load();

		if ( Config.Instance?.Enabled == false ) return;
		LoadModules();

		Console.CreateDefaultCommands();
		UserData.LoadAll();
		UserGroup.LoadAll();
	}

	private static void LoadModules()
	{
		modules.Clear();

		var moduleTypes = TypeLibrary.GetTypes<CommandModule>().Where( m => !m.IsAbstract );
		List<string> disabled = Config.Instance?.DisabledModules ?? new();
		foreach ( var type in moduleTypes )
		{
			var m = type.Create<CommandModule>();
			if(disabled.Contains(type.Name, StringComparer.OrdinalIgnoreCase))
			{
				m.Enabled = false;
			}
			else
			{
				m.Enabled = true;
				m.OnEnabled();
			}

			modules.Add(m);
		}
	}

	public static CommandModule FindModule(string name)
	{
		return modules.FirstOrDefault( m => m.GetType().Name.Contains( name, StringComparison.OrdinalIgnoreCase ) );
	}
	public static void EnableModule( CommandModule module )
	{
		if ( module != null )
		{
			string name = module.GetType().Name;
			if ( module.Enabled )
			{
				Logging.Info( $"Enabled module {name}" );
				module.Enabled = true;
				module.OnEnabled();
			}
			else
			{
				Logging.Message( $"Module {name} is already enabled!", LogLevel.Warn );
				
			}
		}
		else
		{
			Logging.Error( $"Tried to enable invalid module!" );
		}
	}
	public static void EnableModule(string name)
	{
		var module = FindModule( name );
		if(module != null )
		{
			EnableModule( module );
		}
		else
		{
			Logging.Error( $"No module with name {name} exists." );
		}
	}

	public static void DisableModule( CommandModule module )
	{
		if(module != null)
		{
			string name = module.GetType().Name;
			if(!module.Enabled )
			{
				Logging.Info( $"Disabled module {module.GetType().Name}" );
				module.Enabled = false;
				module.OnDisabled();
			}
			else
			{
				Logging.Message( $"Module {name} is already disabled!", LogLevel.Warn );
			}
		}
		else
		{
			Logging.Error( $"Tried to disable invalid module!" );
		}
	}

	public static void DisableModule( string name )
	{
		var module = FindModule( name );
		if ( module != null )
		{
			DisableModule( module );
		}
		else
		{
			Logging.Error( $"No module with name {name} exists." );
		}
	}
	public static void SaveModules()
	{
		var disabledModules = modules.Where( m => !m.Enabled ).Select(m => m.GetType().Name).ToList();
		Config.Instance.DisabledModules = disabledModules;
		Config.Save();
		Logging.Info( $"Saved current module configuration" );
	}
	private void InitConnection( Connection conn )
	{
		Log.Info( $"InitConn {conn}" );
		UserData user;
		if ( !UserData.Exists( conn ) )
		{
			Log.Info( $"Adding user {conn}" );
			UserData.Create( conn );
			user = User.Get( conn );
		}
		else
		{
			user = UserData.Get( conn.SteamId );
			user.Name = conn.DisplayName;
			user.Save();

			user.CreateInstance(conn);
		}

		AdminEvent.Fire( AdminEvent.USER_JOIN, user );
		Logging.Info( $"User {user} joined." );
	}
	void Component.INetworkListener.OnConnected( Connection conn )
	{
		InitConnection( conn );
	}

	void Component.INetworkListener.OnDisconnected(Connection conn)
	{
		if(UserData.Exists( conn ) )
		{
			var user = User.Get( conn );
			UserData.RemoveInstance( conn );
			AdminEvent.Fire( AdminEvent.USER_LEAVE, user );
			Logging.Info( $"User {user} left." );
		}
	}

}
