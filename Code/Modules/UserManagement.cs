using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public class UserManagement : CommandModule
{
	[Command( "userprint" )]
	public void PrintUsers()
	{
		Message.Caller( "Users:" );
		foreach ( var cl in Connection.All )
		{
			var user = User.Get( cl );
			if ( cl == Connection.Local )
				user = User.Get( (ulong)Game.SteamId );

			Message.Caller( $"{user.Name} | Groups: ({string.Join( ",", user.Groups )})" );
		}
	}

	[Command( "usergroup" ), Permission( "breaker.user.group" )]
	public void ManageGroup( [Title( "add/remove" )] string action, TargetUser target, string group )
	{
		switch ( action )
		{
			case "add":
				foreach(var user in target)
				{
					AddGroup( user, group );
				}
				Message.Caller( $"Added {target} to group {group}" );
				Message.Multiple( $"You were added to group {group}!", target );

				break;
			case "remove":
				bool anySuccess = false;
				foreach ( var user in target )
				{
					if ( RemoveGroup( user, group ) )
						anySuccess = true;
				}

				if(anySuccess)
				{
					Message.Caller( $"Removed {target} from group {group}" );
					Message.Multiple( $"You were removed from group {group}!", target );
				}
				else
				{
					Message.Caller( "No valid targets found!" );
				}
				break;
			default:
				Message.Caller( $"Invalid action {action}!", MessageType.Error );
				break;
		}
	}

	[Command( "userperm" ), Permission( "breaker.user.permission" )]
	public void ManagePermissions( [Title("add/remove")] string action, TargetUser target, string permission)
	{
		switch ( action )
		{
			case "add":
				foreach ( var user in target )
				{
					AddPermission( user, permission );
				}
				Message.Caller( $"Added permission {permission} to {target}" );
				Message.Multiple( $"You were given permission \"{permission}\"!", target );

				break;
			case "remove":
				foreach ( var user in target )
				{
					RemovePermission( user, permission );
				}

				Message.Caller( $"Removed permission {permission} from {target}" );
				Message.Multiple( $"You permission \"{permission}\" was removed!", target );
				break;
			default:
				Message.Caller( $"Invalid action {action}!", MessageType.Error );
				break;
		}
	}

	private bool AddGroup( User user, string group )
	{
		if ( UserGroup.Exists( group ) )
		{
			if ( user.Groups.Contains( group ) )
			{
				Message.Caller( $"User is already in group {group}!", MessageType.Error );
				return false;
			}
			user.AddGroup( group );
			user.Save();
			return true;
		}

		Message.Caller( $"Group {group} does not exist!", MessageType.Error );
		return false;
	}

	private bool RemoveGroup( User user, string group )
	{
		if ( user.Groups.Contains( group ) )
		{
			user.RemoveGroup( group );
			user.Save();
			return true;
		}

		Message.Caller( $"User {user.Name} is not in group {group}!", MessageType.Error );
		return false;
	}

	private void AddPermission(User user, string permission )
	{
		user.AddPermission( permission );
		user.Save();
	}

	private bool RemovePermission(User user, string permission)
	{
		if(user.Permissions.Contains( permission ))
		{
			user.RemovePermission( permission );
			user.Save();
			return true;
		}
		else
		{
			Message.Caller( $"User does not have permission {permission}!" );
			return false;
		}
	}
}
