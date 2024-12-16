using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

/// <summary>
/// A module for breaker. Contains commands/features which can be toggled as a group.
/// </summary>
public abstract class CommandModule
{
	[Hide] public bool Enabled { get; internal set; }
	public virtual void OnEnabled()
	{
		AddCommands();
	}
	public virtual void OnDisabled()
	{
		RemoveCommands();
	}

	protected virtual void AddCommands()
	{
		var methods = TypeLibrary.GetType(GetType()).Methods;
		foreach(var m in methods)
		{
			var attr = m.GetCustomAttribute<CommandAttribute>();
			if ( attr == null ) continue;

			CommandDefinition.Create( attr.Key, GetType().Name, m.Name, this );
		}
	}

	protected virtual void RemoveCommands()
	{
		var methods = TypeLibrary.GetType( GetType() ).Methods;
		foreach ( var m in methods )
		{
			var attr = m.GetCustomAttribute<CommandAttribute>();
			if ( attr == null ) continue;
		
			CommandDefinition.Remove( attr.Key );
		}
	}
}
