using Sandbox;
using Sandbox.Rendering;

public sealed class DisplacementTest : Component
{
	[Property]
	private CameraComponent Camera { get; set; }
	
	[Property]
	private Material DebugMaterial { get; set; }

	protected override void OnEnabled()
	{
		CommandList list = new CommandList();
		
		list.Blit( DebugMaterial );
		
		Camera.AddCommandList( list, Stage.AfterOpaque );
	}
	
	// protected override void OnPreRender()
	// {
	// 	// create texture
	// 	var attributes = new RenderAttributes();
	// 	attributes.Set( "Texture", Texture.White );
	// 	Graphics.Blit( DebugMaterial, attributes );
	// }
}
