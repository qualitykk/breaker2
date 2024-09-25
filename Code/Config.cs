using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public class Config
{
	public const string SAVE_FILE = "config.json";
	static Config()
	{
		Load();
	}
	public static Config Load()
	{
		Instance = DataFile.Load<Config>( SAVE_FILE );
		return Instance;
	}
	public static Config Instance { get; set; }
	public bool Enabled { get; set; } = true;
	public void Save()
	{
		DataFile.Save( SAVE_FILE, this );
	}
}
