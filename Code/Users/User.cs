using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Breaker;

public partial struct User : IEquatable<User>
{
	public static User Get(ulong steamId)
	{
		var conn = Connection.All.FirstOrDefault( c => c.SteamId == steamId );
		if(conn != null)
		{
			return Get( conn );
		}

		return default;
	}

	public static User Get(Connection conn)
	{
		return UserData.GetInstance(conn);
	}
	public const char SPLIT_CHAR = ';';
	public ulong SteamId { get; set; }
	public string Name { get; set; }
	public Guid ConnectionId { get; set; }
	public string Groups { get; set; }
	public string Permissions { get; set; }
	public Connection GetConnection() => Connection.Find( ConnectionId );
	public IEnumerable<UserGroup> GetGroupInfo()
	{
		return Groups.Split( SPLIT_CHAR ).Select( id => UserGroup.All[id] );
	}

	public List<string> GetPermissions()
	{
		var perms = new List<string>();
		foreach ( var group in GetGroupInfo() )
		{
			perms.AddRange( group.Permissions );
		}

		if(!string.IsNullOrEmpty(Permissions))
		{
			perms.AddRange( Permissions.Split( SPLIT_CHAR ) );
		}

		return perms;
	}

	public bool CanTarget( User target )
	{
		var targetGroups = target.GetGroupInfo();
		foreach ( var group in GetGroupInfo() )
		{
			foreach ( var targetGroup in targetGroups )
			{
				if ( group.CanTarget( targetGroup ) )
					return true;
			}
		}
		return false;
	}

	public bool CanTarget( UserGroup other )
	{
		foreach ( var group in GetGroupInfo() )
		{
			if ( group.CanTarget( other ) )
				return true;
		}
		return false;
	}
	
	public bool HasPermission(string permission)
	{
		if ( string.IsNullOrEmpty( permission ) ) return true;

		var userPerms = GetPermissions();
		foreach ( var wildcard in userPerms.Where( p => p.EndsWith( ".*" ) ) )
		{
			if ( CheckWildcard( wildcard, permission ) )
				return true;
		}

		return userPerms.Contains( permission );
	}

	public bool HasPermission(IEnumerable<string> permissions)
	{
		if ( !Networking.IsActive || GetConnection()?.IsHost == true ) return true;
		if ( permissions == default ) return true;

		var userPerms = GetPermissions();
		if ( userPerms.Count == 0 ) return !permissions.Any();

		foreach(var permission in permissions)
		{
			foreach ( var wildcard in userPerms.Where( p => p.EndsWith( ".*" ) ) )
			{
				if ( CheckWildcard( wildcard, permission ) )
					continue;
			}

			if(!userPerms.Contains( permission ))
			{
				return false;
			}
		}

		return true;
	}

	public void AddPermission(string perm)
	{
		Permissions += SPLIT_CHAR + perm;
	}

	public void AddGroup( string group )
	{
		Groups += SPLIT_CHAR + group;
	}
	public void RemovePermission(string permission)
	{
		var perms = Permissions.Split( SPLIT_CHAR ).ToList();
		if(perms.Contains( permission ))
			perms.Remove( permission );
		Permissions = string.Join( SPLIT_CHAR, perms );
	}
	public void RemoveGroup(string group)
	{
		var groups = Groups.Split( SPLIT_CHAR ).ToList();
		if ( groups.Contains( group ) )
			groups.Remove( group );
		Groups = string.Join( SPLIT_CHAR, groups );
	}

	/// <summary>
	/// Check if permission (wildcard) allows a sub-permission (permission)
	/// </summary>
	private static bool CheckWildcard( string wildcard, string permission )
	{
		var wildcardParts = wildcard.Split( '.' );
		var permissionParts = permission.Split( '.' );
		if ( wildcardParts.Length > permissionParts.Length )
			return false;

		// Check if the generic permission matches the permission we are looking for
		for ( int i = 0; i < wildcardParts.Length; i++ )
		{
			if ( wildcardParts[i] != permissionParts[i] )
				break;

			if ( i == wildcardParts.Length - 1 )
				return true;
		}

		return false;
	}

	public void Save()
	{
		UserData data = (UserData)this;
		data.Save();
	}

	public override string ToString()
	{
		return $"{Name} ({SteamId})";
	}

	public override bool Equals( object obj )
	{
		return obj is User user && Equals( user );
	}

	public bool Equals( User other )
	{
		return SteamId == other.SteamId;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine( SteamId );
	}

	public static bool operator ==( User current, User other ) => current.SteamId == other.SteamId;
	public static bool operator !=( User current, User other ) => current.SteamId == other.SteamId;
}

[AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
public class PermissionAttribute : Attribute
{
	public string Permission { get; private set; }
	/// <summary>
	/// Dont check for this permission when executing the command
	/// </summary>
	public bool ManualEnforcement { get; init; }
	public PermissionAttribute( string permission, bool manualEnforcement = false )
	{
		Permission = permission;
		ManualEnforcement = manualEnforcement;
	}

	public static implicit operator string( PermissionAttribute attr ) => attr.Permission;
}
