namespace Breaker;

public class UserData
{
	const string USER_DB = "users";
	[HostSync] private static NetDictionary<Guid, User> activeUsers { get; set; } = new();
	[HostSync] private static NetDictionary<ulong, UserData> users { get; set; } = new();
	public static IReadOnlyDictionary<ulong, UserData> All => users.AsReadOnly();
	public static User GetInstance( Connection conn ) => activeUsers[conn.Id];
	public static UserData Get( ulong steamId )
	{
		if(users.ContainsKey(steamId)) return users[steamId];
		return null;
	}
	public static UserData Get( User instance ) => users[instance.SteamId];
	[Authority]
	public static void LoadAll()
	{
		Logging.Info( $"Reading users..." );
		var data = Database.Load<Dictionary<ulong, UserData>>( USER_DB, new() );
		users.Clear();

		if ( data.Count > 0 )
		{
			foreach ( var kv in data )
			{
				users.Add( kv );
			}
		}
		Logging.Info( $"Loaded data of {users.Count} users." );
	}
	[Authority]
	public static void SaveAll()
	{
		if ( users?.Count == 0 )
			users = new();

		Logging.Info( $"Saved {users.Count} users." );
		Database.Save( USER_DB, users );
	}
	[Authority]
	public static void Create( Connection conn )
	{
		if ( users.ContainsKey( conn.SteamId ) )
		{
			Logging.Error( $"Tried to register user with duplicate id {conn.SteamId}! " );
			return;
		}

		UserData user = new()
		{
			SteamId = conn.SteamId,
			Groups = new() { Config.Instance?.DefaultUserGroup },
			Name = conn.DisplayName,
		};
		user.CreateInstance( conn );

		Logging.Info( $"Creating user data for {user.SteamId}..." );
		SaveAll();
	}
	[Authority]
	public static void RemoveInstance(Connection conn)
	{
		if(!activeUsers.ContainsKey(conn.Id))
		{
			return;
		}

		activeUsers.Remove( conn.Id );
	}
	public static bool Exists( ulong id ) => users.ContainsKey( id );
	public static bool Exists( Connection conn ) => Exists( conn.SteamId );
	public static bool Online(ulong id)
	{
		var conn = Connection.All.FirstOrDefault( c => c.SteamId == id );
		if ( conn == null ) return false;

		return activeUsers.ContainsKey( conn.Id );
	}

	public ulong SteamId { get; set; }
	public string Name { get; set; }
	public List<string> Groups { get; set; }
	public List<string> Permissions { get; set; }
	public User CreateInstance(Connection conn)
	{
		if(activeUsers.TryGetValue(conn.Id, out var existing))
		{
			return existing;
		}

		User instance = new()
		{
			SteamId = SteamId,
			Name = Name,
			Groups = string.Join( ';', Groups ),
			Permissions = string.Join( ';', Permissions ),
			ConnectionId = conn.Id
		};
		activeUsers.Add( conn.Id, instance);
		return instance;
	}
	public void Save()
	{
		SaveAll();
	}

	public static implicit operator UserData(User instance)
	{
		UserData data = new()
		{
			SteamId = instance.SteamId,
			Name = instance.Name,
			Groups = instance.Groups.Split(';').ToList(),
			Permissions = instance.Permissions.Split(';').ToList(),
		};

		return data;
	}
}
