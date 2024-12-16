using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public partial class ModerationModule : CommandModule
{
	public override void OnEnabled()
	{
		base.OnEnabled();
		LoadBans();

		AdminEvent.Hook( AdminEvent.USER_JOIN, CheckUserBan );
	}

	[Command("kick"), Permission("usermod.kick")]
	public static void Kick(TargetUser targets, string reason = "No reason provided.")
	{
		Log.Info( targets );
		if ( !targets.Any() ) return;

		foreach(var user in targets)
		{
			Disconnect( user, reason );
		}

		Message.Caller( $"Kicked {targets}" );
		Logging.Info( $"{targets} was kicked. Reason: {reason}" );
	}

	public static void Disconnect(User user, string reason = "")
	{
		using(Rpc.FilterInclude(user.GetConnection()))
		{
			Disconnect( reason );
		}
	}
	[Broadcast]
	private static void Disconnect(string reason = "")
	{
		Log.Info( $"You've been disconnected! Reason: {reason}" );
		Game.Close();
	}
}
