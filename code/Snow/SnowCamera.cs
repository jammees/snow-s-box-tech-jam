using Sandbox;

public sealed class SnowCamera : Component
{
	[Property]
	private CameraComponent Camera { get; set; }

	[Property]
	private Vector2 CustomSize
	{
		get => _customSize;
		set
		{
			_customSize = value;
			Camera.CustomSize = _customSize;
		}
	}

	private Vector2 _customSize;
	
	protected override void OnStart()
	{
		Camera.CustomSize = new Vector2( 64f, 64f );
	}

	protected override void OnUpdate()
	{

	}
}
