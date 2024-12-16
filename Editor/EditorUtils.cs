using Editor;

public static class EditorUtils
{
	[Menu( "Editor", "Breaker/Open Data Folder" )]
	public static void OpenDataFolder()
	{
		EditorUtility.OpenFolder( Sandbox.FileSystem.Data.GetFullPath( FileDataStorage.PATH ) );
	}
}
