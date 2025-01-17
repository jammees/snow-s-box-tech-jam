using Sandbox;

public sealed class ReturnToMainDemo : Component
{
	protected override void OnUpdate()
	{
		if ( Input.Pressed( "Return" ) )
		{
			Scene.LoadFromFile( "scenes/terrain.scene" );
		}
	}
}
