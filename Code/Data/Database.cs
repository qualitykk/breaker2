using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public static class Database
{
	public static IDataStorage Storage { get; private set; }
	internal static void Init()
	{
		Storage = new FileDataStorage();
	}
	public static T Load<T>(string path, T defaultValue = default)
	{
		return Storage.Read( path, defaultValue ) ?? defaultValue;
	}
	public static void Save(string path, object data)
	{
		Storage?.Write( path, data );
	}
}
