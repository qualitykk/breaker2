
namespace Breaker;

public partial class ModerationModule
{
	const string BAN_DB = "bans";
	[HostSync] private static NetList<BanEntry> bans { get; set; } = new();
	[Authority]
	public static void LoadBans()
	{
		var data = Database.Load<List<BanEntry>>(BAN_DB) ?? new();
		if(data.Count > 0)
		{
			bans.Clear();
			foreach ( var entry in data )
			{
				bans.Add( entry );
			}
		}
		Logging.Info( $"Loaded data of {bans?.Count} bans." );
	}
	[Authority]
	public static void SaveBans()
	{
		List<BanEntry> data = bans?.ToList();
		if ( data?.Count == 0 )
			data = new();

		Logging.Info( $"Saved {data.Count} bans." );
		Database.Save( BAN_DB, data );
	}
	public static bool IsBanned(ulong user)
	{
		return bans.Any(b => b.SteamId == user && b.IsBanned() );
	}
	public void AddBan(ulong user, long time, string reason = "No reason given.")
	{
		var banTime = TimeSpan.FromSeconds( time );
		BanEntry entry = new()
		{
			SteamId = user,
			Timestamp = DateTime.Now.Ticks,
			Duration = banTime.Ticks,
			Reason = reason
		};

		if(time > 0)
		{
			Logging.Info( $"{user} was banned for {banTime.Humanize(1)}. Reason: {reason}" );
		}
		else
		{
			Logging.Info( $"{user} was banned permanently. Reason: {reason}" );
		}

		bans.Add( entry );
		SaveBans();
	}
	public void RemoveBans(ulong steamId)
	{
		var userBans = bans.Where(e => e.SteamId == steamId ).ToList();
		if(userBans?.Count == 0 )
		{
			foreach(var entry in userBans)
			{
				bans.Remove( entry );
			}
		}
		Logging.Info( $"Removed {userBans?.Count ?? 0} bans from user {steamId}" );
	}

	[Command("ban"), Permission( "breaker.user.ban" )]
	public void Ban(ulong steamid, long time = 0, string reason = "No reason given.")
	{
		var userType = Steam.CategorizeSteamId( steamid );
		if(userType != SteamId.AccountTypes.Individual)
		{
			Message.Caller( "Invalid SteamID entered!" );
			return;
		}

		AddBan( steamid, time, reason );
		if( UserData.Exists(steamid))
		{
			if(UserData.Online(steamid))
			{
				var user = User.Get( steamid );
				Disconnect( user, $"Banned: {reason}" );
				Message.Caller( $"Banned {user.Name}" );
			}
			else
			{
				var data = UserData.Get( steamid );
				Message.Caller( $"Banned {data.Name}" );
			}
		}
		else
		{
			Message.Caller( $"Banned user with steamid {steamid}" );
		}

	}

	[Command("unban"), Permission("breaker.user.ban")]
	public void Unban(ulong steamId)
	{
		RemoveBans( steamId );
		Message.Caller( $"Unbanned {steamId}" );
	}

	void CheckUserBan( object[] args)
	{
		if ( args.Length <= 0 || args[0] is not User user )
			return;

		var userEntries = bans.Where( b => b.SteamId == user.SteamId ).ToArray();
		foreach(var entry in userEntries)
		{
			if(!entry.IsBanned())
				bans.Remove(entry );
		}
	}
}

public struct BanEntry
{
	public ulong SteamId { get; set; }
	public string Reason { get; set; }
	public long Timestamp { get; set; }
	public long Duration { get; set; }

	public bool IsBanned()
	{
		return Duration == 0 || DateTime.Now.Ticks <= Timestamp + Duration;
	}
}
