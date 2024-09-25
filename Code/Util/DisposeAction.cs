using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;
public struct DisposeAction : IDisposable
{
	public Action Action;

	public DisposeAction( Action action )
	{
		Action = action;
	}

	public void Dispose()
	{
		Action?.Invoke();
		Action = null;
	}

	public static IDisposable Create( Action action )
	{
		return new DisposeAction( action );
	}
}
