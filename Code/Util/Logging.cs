using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public static partial class Logging
{
	private static List<ILogger> loggers = new();
	public static LogLevel Level => Config.Instance?.LogLevel ?? LogLevel.Debug;
	public static void RegisterLogger( ILogger logger )
	{
		if ( logger == null ) return;

		loggers.Add( logger );
	}

	public static void ClearLoggers()
	{
		loggers.Clear();
	}
	public static void Message( object message, LogLevel level = LogLevel.Debug )
	{
		if (level < Level )
		{
			return;
		}

		foreach(var l in loggers)
		{
			l.OnLog( message.ToString(), level );
		}

		if(level >= LogLevel.Error)
		{
			Log.Error( $"{Globals.PREFIX} [{level}] {message}" );
		}
		else if(level >= LogLevel.Warn)
		{
			Log.Warning( $"{Globals.PREFIX} [{level}] {message}" );
		}
		else
		{
			Log.Info( $"{Globals.PREFIX} [{level}] {message}" );
		}
	}
	public static void Info( object message )
	{
		Message( message, LogLevel.Info );
	}

	public static void Error( object message )
	{
		Message( message, LogLevel.Error );
	}

	public static bool IsError(this LogLevel level)
	{
		return level >= LogLevel.Error;
	}
}
public enum LogLevel
{
	Debug = -1,
	Info = 0,
	Warn,
	Error,
	Critical
}
public interface ILogger
{
	public void OnLog( string message, LogLevel level = LogLevel.Info );
}
