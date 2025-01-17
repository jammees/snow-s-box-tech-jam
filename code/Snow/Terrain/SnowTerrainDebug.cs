using Sandbox;

public sealed class SnowTerrainDebug : Component
{
	[Property]
	[Range(0, 40, clamped: true)]
	private float SnowHeight { get; set; } = 20f;
	
	[Property] private float TerrainHeight { get; set; } = 2000f;
	
	[Property]
	[Range(0, 1, clamped: true)]
	private float CurrentHeight { get; set; } = 0f;
	
	[Property] private GameObject Plane { get; set; }
	[Property] private GameObject Box { get; set; }
	
	protected override void OnUpdate()
	{
		Vector3 pos = Plane.WorldPosition;
		float realHeight = TerrainHeight * CurrentHeight;
		float farZ = TerrainHeight + SnowHeight * 2;
		
		DebugOverlay.ScreenText(
			new Vector2( 25, 200 ),
			$"FarZ: {farZ}",
			flags: TextFlag.Left);
		
		DebugOverlay.Box(
			pos.WithZ( pos.z + SnowHeight * 0.5f ),
			(Vector3.One * 190f).WithZ( SnowHeight ) );

		DebugOverlay.Box( 
			LocalPosition.WithZ( LocalPosition.z + farZ * 0.5f - SnowHeight ),
			(Vector3.One * 200f).WithZ( farZ ),
			Gizmo.Colors.Blue);
		
		DebugOverlay.ScreenText(
			new Vector2( 25, 50 ),
			$"Snow Height: {SnowHeight}",
			flags: TextFlag.Left);
		
		DebugOverlay.ScreenText(
			new Vector2( 25, 75 ),
			$"Current Height: {realHeight}",
			flags: TextFlag.Left);

		// WorldPosition = WorldPosition.WithZ( TerrainHeight - realHeight - TerrainHeight + SnowHeight );
		Plane.LocalPosition = Plane.LocalPosition.WithZ(realHeight);

		float boxDepth = ((Box.WorldPosition - WorldPosition.WithZ( WorldPosition.z - SnowHeight )) / farZ).z;
		boxDepth = boxDepth.Clamp( 0, 1 );
		
		DebugOverlay.ScreenText(
			new Vector2( 200, 100 ),
			$"Box Raw Depth: {boxDepth}",
			flags: TextFlag.Left);
		
		boxDepth *= farZ;
		
		DebugOverlay.ScreenText(
			new Vector2( 25, 100 ),
			$"Box Depth: {boxDepth}",
			flags: TextFlag.Left);

		float snowSurfaceHeight = realHeight + SnowHeight * 2;
		
		DebugOverlay.ScreenText(
			new Vector2( 25, 125 ),
			$"Snow Surface Height: {snowSurfaceHeight}",
			flags: TextFlag.Left);

		float penetration = snowSurfaceHeight - (boxDepth);
		
		DebugOverlay.ScreenText(
			new Vector2( 25, 150 ),
			$"Snow Penetration: {penetration}",
			flags: TextFlag.Left);

		bool isPenetrating = penetration > 0;
		
		DebugOverlay.ScreenText(
			new Vector2( 25, 175 ),
			$"Snow Penetration State: {isPenetrating}",
			flags: TextFlag.Left);
	}
}
