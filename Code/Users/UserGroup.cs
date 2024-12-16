using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;
public partial class UserGroup
{
	#region Persistent User Groups
	const string GROUP_DB = "groups";
	public static IReadOnlyDictionary<string, UserGroup> All => all.AsReadOnly();
	[HostSync] private static NetDictionary<string, UserGroup> all { get; set; } = new();
	private static readonly Dictionary<string, UserGroup> defaultGroups = new()
	{
		{ "user", new() {Id = "user", Weight = 0 } },
		{ "admin", new() {Id = "admin", Weight = 100, Permissions = new() { "*" } } }
	};
	[Authority]
	public static void LoadAll()
	{
		Logging.Info( $"Reading groups..." );
		all.Clear();
		var data = Database.Load<List<UserGroup>>( GROUP_DB );
		if ( data == null )
		{
			foreach ( var kv in defaultGroups )
				all.Add( kv );

			SaveAll();
		}
		else
		{
			foreach ( var entry in data )
				all.Add( entry.Id, entry );
		}

		Logging.Info( $"Loaded data of {all.Count} groups." );
	}

	[Authority]
	public static void SaveAll()
	{
		if ( all.Count == 0 )
		{
			Logging.Message( $"No groups exist, creating default groups..." );
			foreach ( var kv in defaultGroups )
				all.Add( kv );
		}

		Database.Save( GROUP_DB, all.Values.ToList() );
		Logging.Message( $"Saved {all.Count} groups." );
	}

	[Authority]
	public static void Create( UserGroup group )
	{
		if ( all.ContainsKey( group.Id ) )
		{
			Logging.Error( $"Tried to register group with duplicate id {group.Id}!" );
			return;
		}

		all.Add( group.Id, group );

		SaveAll();
	}

	[Authority]
	public static void Remove( string id )
	{
		if ( !all.ContainsKey( id ) )
		{
			Logging.Error( $"Tried to remove group with id {id} which doesnt exist!" );
			return;
		}

		all.Remove( id );

		SaveAll();
	}

	public static void Remove( UserGroup group ) => Remove( group.Id );
	public static bool Exists( string id ) => all.ContainsKey( id );
	#endregion
	public static UserGroup GetDefault()
	{
		if ( !string.IsNullOrEmpty( Config.Instance?.DefaultUserGroup ) )
		{
			if ( All.TryGetValue( Config.Instance.DefaultUserGroup, out var group ) )
				return group;
		}

		return all.Select( kv => kv.Value ).OrderBy( g => g.Weight ).First();
	}

	public string Id { get; set; }
	public string Name { get; set; }

	/// <summary>
	/// Decides what users may be targeted by members of this group.
	/// Users can always target other Users with a weight lower then their own.
	/// </summary>
	public int Weight { get; set; }
	public List<string> Permissions { get; set; } = new();
	public UserGroup()
	{
		// Required for JSON
	}

	public UserGroup( string id, int weight, List<string> permissions )
	{
		Id = id;
		Weight = weight;
		Permissions = permissions;
	}
	public void Save()
	{
		SaveAll();
	}

	public bool CanTarget( UserGroup target )
	{
		return this == target || Weight > target.Weight;
	}
}
