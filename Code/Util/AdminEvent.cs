namespace Breaker;

public static class AdminEvent
{
	public const string USER_JOIN = "user_join";
	public const string USER_LEAVE = "user_leave";

	public delegate void EventCallback( object[] parameters );
	private static Dictionary<string, List<EventCallback>> eventListeners = new();
	public static void Fire(string name, params object[] parameters)
	{
		if(!eventListeners.TryGetValue(name, out var listeners))
		{
			return;
		}

		if(eventListeners.Count > 0)
		{
			foreach(var listener in listeners)
			{
				listener.Invoke(parameters);
			}
		}
	}
	public static void Hook(string name, EventCallback callback)
	{
		if(eventListeners.TryGetValue(name,out var listeners))
		{
			listeners.Add( callback );
		}
		else
		{
			eventListeners.Add( name, new() { callback } );
		}
	}
	public static void Reset()
	{
		eventListeners.Clear();
	}
}
