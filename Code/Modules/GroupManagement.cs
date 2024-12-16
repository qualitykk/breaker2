using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public class GroupManagementModule : CommandModule
{
	[Command( "groupget" )]
	public void Print( string name = "" )
	{
		Message.Caller( "Groups:" );
		foreach ( var kv in UserGroup.All )
		{
			if ( kv.Key.Contains( name ) )
				Message.Caller( $"- {kv.Key} ({kv.Value.Weight}) [{string.Join( ", ", kv.Value.Permissions )}]" );
		}
	}

	[Command( "groupadd" ), Permission( "breaker.group.create" )]
	public void Add( string id, int weight, string permissions = null )
	{
		UserGroup group = new( id, weight, permissions.Split( ';').ToList() );

		if ( UserGroup.Exists( id ) )
		{
			Message.Caller( $"Group {id} already exists!", MessageType.Error );
			return;
		}

		UserGroup.Create( group );
		Message.Caller( $"Created group {id} with weight {weight} and {permissions.Count()} permissions." );
	}

	[Command( "groupremove" ), Permission( "breaker.group.remove" )]
	public static void Remove( string id )
	{
		if ( !UserGroup.Exists( id ) )
		{
			Message.Caller( $"Group {id} does not exist!", MessageType.Error );
			return;
		}
		var group = UserGroup.All[id];
		if ( !CommandContext.Caller.CanTarget( group ) )
		{
			Message.Caller( $"You dont have permission to edit this group!", MessageType.Error );
			return;
		}

		UserGroup.Remove( group );
		Message.Caller( $"Removed group {id}." );
	}

	[Command( "groupperms" ), Permission( "breaker.group.edit.permissions" )]
	public static void EditPermissions( [Title( "add/remove" )] string action, string id, string permission )
	{
		if ( !UserGroup.Exists( id ) )
		{
			Message.Caller( $"Group {id} does not exist!", MessageType.Error );
			return;
		}

		var group = UserGroup.All[id];
		if ( !CommandContext.Caller.CanTarget( group ) )
		{
			Message.Caller( $"You dont have permission to edit this group!", MessageType.Error );
			return;
		}

		switch ( action )
		{
			case "add":
				if ( group.Permissions.Contains( permission ) )
				{
					Message.Caller( $"Group {id} already has permission {permission}!", MessageType.Error );
					return;
				}
				group.Permissions.Add( permission );
				group.Save();
				Message.Caller( $"Added permission {permission} to group {id}." );
				break;
			case "remove":
				if ( !group.Permissions.Contains( permission ) )
				{
					Message.Caller( $"Group {id} does not have permission {permission}!", MessageType.Error );
					return;
				}
				group.Permissions.Remove( permission );
				group.Save();
				Message.Caller( $"Removed permission {permission} from group {id}." );
				break;
			default:
				Message.Caller( $"Invalid action {action}!", MessageType.Error );
				break;
		}
	}
	[Command( "groupweight" ), Permission( "breaker.group.edit.weight" )]
	public static void EditWeight( string id, int weight )
	{
		if ( !UserGroup.Exists( id ) )
		{
			Message.Caller( $"Group {id} does not exist!", MessageType.Error );
			return;
		}

		var group = UserGroup.All[id];
		if ( !CommandContext.Caller.CanTarget( group ) )
		{
			Message.Caller( $"You dont have permission to edit this group!", MessageType.Error );
			return;
		}
		group.Weight = weight;
		group.Save();
		Message.Caller( $"Set weight of group {id} to {weight}." );
	}

	[Command( "groupsettings" ), Permission( "breaker.group.edit.settings" )]
	public static void EditSettings( string id, string setting, string value )
	{
		if ( !UserGroup.Exists( id ) )
		{
			Message.Caller( $"Group {id} does not exist!", MessageType.Error );
			return;
		}

		var group = UserGroup.All[id];
		if ( !CommandContext.Caller.CanTarget( group ) )
		{
			Message.Caller( $"You dont have permission to edit this group!", MessageType.Error );
			return;
		}

		switch ( setting )
		{
			case "name":
				group.Name = value;
				break;
			/*
			case "namecolor":
				group.NameColor = value;
				break;
			case "prefix":
				group.Prefix = value;
				break;
			case "prefixcolor":
				group.PrefixColor = value;
				break;
			case "icon":
				group.Icon = value;
				break;
			*/
			default:
				Message.Caller( $"Invalid setting {setting}!", MessageType.Error );
				return;
		}
		group.Save();
		Message.Caller( $"Set {setting} of group {id} to {value}." );
	}
}
