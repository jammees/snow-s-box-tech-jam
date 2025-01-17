namespace Sandbox.Snow.Terrain;

public sealed class SnowTerrainCommands: Component
{
	[Property] [Group( "References" )] private GameObject Objects { get; set; }
	
	internal static SnowTerrainCommands Instance;
	
	protected override void OnStart()
	{
		if ( IsProxy )
			return;
		
		Log.Info(
			"Greetings! If you are looking for commands, " +
			"all of them are prefixed with \"snow\"."
			);
		Instance = this;
	}

	[ConVar("snow_objects_enabled")]
	private static bool ObjectsEnabled
	{
		get
		{
			if ( Instance.Objects is null ) return false;

			return Instance.Objects.Enabled;
		}
		set
		{
			if ( Instance.Objects is null ) return;

			Instance.Objects.Enabled = value;
		}
	}
	
	// [ConCmd("snow_old_demo")]
	// private static void OldDemo()
	// {
	// 	if ( Instance is null ) return;
	// 	Instance.Scene.LoadFromFile( "scenes/plane.scene" );
	// }
}
