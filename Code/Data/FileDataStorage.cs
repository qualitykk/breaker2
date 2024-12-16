
using System.IO;

namespace Breaker;

public class FileDataStorage : IDataStorage
{
	public const string PATH = "/breaker2/";
	const string FILE_EXTENSION = ".json";
	public FileDataStorage()
	{
		if ( FileSystem.Data?.DirectoryExists( PATH ) == false )
		{
			FileSystem.Data.CreateDirectory( PATH );
		}
	}
	public T Read<T>( string id, T defaultValue = default )
	{
		string path = PATH + id + FILE_EXTENSION;
		return FileSystem.Data.ReadJsonOrDefault( path, defaultValue ) ?? defaultValue;
	}

	public bool Write<T>( string id, T value )
	{
		string path = PATH + id + FILE_EXTENSION;
		string dir = Path.GetDirectoryName( path );
		if ( FileSystem.Data.DirectoryExists( dir ) )
		{
			FileSystem.Data.CreateDirectory( dir );
		}

		try
		{
			FileSystem.Data.WriteJson( path, value );
			Logging.Message( $"Writing data {value} to {path}" );
			return true;
		}
		catch
		{
			return false;
		}
	}
}
