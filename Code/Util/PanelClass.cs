using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

internal static class PanelClass
{
	public static string If( string c, bool value )
	{
		return value ? c : "";
	}

	public static string If( string c, Func<bool> action )
	{
		if ( action == null ) return "";

		return action() ? c : "";
	}
	public static string If( string c, object value, object expected )
	{
		if ( value.Equals( expected ) )
		{
			return c;
		}

		return "";
	}
}
