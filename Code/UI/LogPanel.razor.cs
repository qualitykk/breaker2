using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;
[Category("Breaker")]
public partial class LogPanel : PanelComponent, ILogger
{
	private class LogLine
	{
		public string Message;
		public LogLevel Level;
		public TimeSince TimeSinceAdded;
	}
	[Property] public float InfoFadeTime { get; set; } = 20f;
	[Property] public float ErrorFadeTime { get; set; } = 60f;
	[Property, InputAction] public string ToggleAction { get; set; }
	[Property, InputAction] public string CursorActivateAction { get; set; }
	private List<LogLine> lines = new();
	bool hidden = false;
	bool collapsed = false;
	bool cursor = false;
	protected override void OnEnabled()
	{
		base.OnEnabled();
		BindClass( "hidden", () => hidden );
		BindClass( "collapsed", () => collapsed );
		BindClass( "cursor", () => cursor );

		Logging.RegisterLogger( this );
	}
	protected override void OnUpdate()
	{	
		
		if(Input.Pressed(ToggleAction))
		{
			Toggle();
		}

		if(Input.Pressed(CursorActivateAction))
		{
			ToggleCursor();
		}

		foreach(var line in lines.ToArray())
		{
			float maxtime = line.Level.IsError() ? ErrorFadeTime : InfoFadeTime;
			if(line.TimeSinceAdded >= maxtime)
			{
				lines.Remove( line );
			}
		}
	}
	public void OnLog( string message, LogLevel level = LogLevel.Info )
	{
		lines.Add( new()
		{
			Message = message,
			Level = level,
			TimeSinceAdded = 0
		} );
	}

	[ConCmd("brk_log_toggle")]
	public void Toggle()
	{
		if ( Network.IsProxy ) return;

		hidden = !hidden;
	}

	private void ToggleCollapse()
	{
		collapsed = !collapsed;
	}

	private void ToggleCursor()
	{
		cursor = !cursor;
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( RealTime.Now );
	}
}
