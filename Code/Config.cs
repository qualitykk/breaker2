using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public class Config
{
	const string SAVE_FILE = "config";
	[Authority]
	public static void Load()
	{
		Instance = Database.Load<Config>( SAVE_FILE );
		if(Instance == null)
		{
			Instance = new();
			Save();
		}
	}
	[Authority]
	public static void Save()
	{
		Database.Save( SAVE_FILE, Instance ?? new() );
	}
	[HostSync] public static Config Instance { get; set; }
	public bool Enabled { get; set; } = true;
	public LogLevel LogLevel { get; set; } = LogLevel.Warn;
	public string DefaultUserGroup { get; set; } = "user";
	public List<string> DisabledModules { get; set; } = new();
}

[Category("Breaker")]
[Title("Breaker Configuration")]
public class ConfigComponent : Component
{
	[Property, InlineEditor(Label = false)] public Config LocalConfig { get; set; }
	protected override void OnEnabled()
	{
		Apply();
	}

	[Button(icon: "save")]
	private void Apply()
	{
		Config.Instance = LocalConfig;
		Config.Save();
	}

	[Button(icon: "file_download")]
	private void Update()
	{
		LocalConfig = Config.Instance;
	}
}
