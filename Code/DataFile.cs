using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public static class DataFile
{
	const string PATH = "/breaker2/";
	static readonly BaseFileSystem fs;
	static DataFile()
	{
		if(!FileSystem.Mounted.DirectoryExists(PATH))
		{
			FileSystem.Mounted.CreateDirectory(PATH);
		}

		fs = FileSystem.Mounted.CreateSubSystem( PATH );
	}

	public static T Load<T>(string path)
	{
		return fs.ReadJsonOrDefault<T>( path );
	}
	public static void Save(string path, object data)
	{
		string dir = Path.GetDirectoryName( path );
		if (fs.DirectoryExists( dir ))
		{
			fs.CreateDirectory( dir );
		}

		fs.WriteJson( path, data );
	}
}
