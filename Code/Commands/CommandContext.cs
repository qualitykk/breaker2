using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public class CommandContext
{
	public static CommandContext Current { get; set; }
	public static User Caller => Current?.User ?? default;
	public static Connection Connection => Current?.UserConnection;
	public User User { get; set; }
	public Connection UserConnection { get; set; }

	public static IDisposable Push(CommandContext ctx)
	{
		var previous = Current;
		Current = ctx;

		return DisposeAction.Create( () =>
		{
			Current = previous;
		} );
	}
}
