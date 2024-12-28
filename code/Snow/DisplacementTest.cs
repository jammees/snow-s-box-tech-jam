using System;
using Sandbox;
using Sandbox.Rendering;

public sealed class DisplacementTest : Component
{
	[Property]
	private CameraComponent Camera { get; set; }
	
	[Property]
	private Model ModelToRender { get; set; }

	protected override void OnStart()
	{
		var sceneObject = new SceneCustomObject( Scene.SceneWorld );
		sceneObject.RenderOverride = OnRender;

		Camera.AddHookAfterTransparent( "w", 0, camera =>
		{
			Graphics.GrabDepthTexture( "T", sceneObject.Attributes );
		} );
	}

	private void OnRender( SceneObject obj )
	{
		Graphics.DrawModel( ModelToRender, new Transform( new Vector3( 0f, 0f, 30f ) ), obj.Attributes );
	}
}
