namespace Sandbox;

public sealed class PlayerManager : Component, Component.INetworkSpawn
{
	[Property]
	private CameraComponent Camera { get; set; }

	[Property]
	private float NameplateHeight { get; set; } = 0f;
	
	private static PlayerManager Instance;

	protected override void DrawGizmos()
	{
		if ( Gizmo.IsSelected is false )
			return;
		
		Gizmo.Draw.LineSphere( Vector3.Up * NameplateHeight, 2, 6 );
	}

	protected override void OnStart()
	{
		Camera.Enabled = !IsProxy;

		if ( IsProxy )
			return;

		Instance = this;
	}

	public void OnNetworkSpawn( Connection owner )
	{
		GameObject nameplateContainer = new( GameObject, name: "Nameplate" );
		nameplateContainer.LocalPosition = Vector3.Up * NameplateHeight;

		WorldPanel panel = nameplateContainer.AddComponent<WorldPanel>();
		panel.LookAtCamera = true;
		panel.PanelSize = new Vector2( 600f, 120f );

		Nameplate playerNameplate = nameplateContainer.AddComponent<Nameplate>();
		playerNameplate.MyStringValue = owner.DisplayName;
	}
}
