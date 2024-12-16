using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public interface IDataStorage
{
	public T Read<T>( string id, T defaultValue = default );
	public bool Write<T>(string id, T value );
}
