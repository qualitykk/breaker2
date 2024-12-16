using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public static class Message
{
	public static void Caller(string message, MessageType type = MessageType.Info)
	{
		Single(message, CommandContext.Connection ?? Connection.Host, type);
	}

	public static void Single( string message, Connection conn, MessageType type = MessageType.Info )
	{
		if ( Networking.IsActive && conn?.SteamId != (ulong)Game.SteamId ) return;

		Log.Info( message );
	}

	public static void Multiple( string message, IEnumerable<Connection> connections, MessageType type = MessageType.Info )
	{
		foreach(var conn in connections)
		{
			Single( message, conn, type );
		}
	}
	public static void Multiple(string message, IEnumerable<User> users, MessageType type = MessageType.Info )
	{
		foreach(var user in users)
		{
			Single(message, user.GetConnection(), type );
		}
	}
	public static void Multiple( string message, MessageType type = MessageType.Info )
	{
		Multiple( message, Connection.All.ToArray(), type );
	}
}

public enum MessageType
{
	Info,
	Error,
	Announcement
}
