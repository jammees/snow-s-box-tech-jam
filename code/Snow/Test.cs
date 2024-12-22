using Sandbox;

namespace Sandbox.Snow;

public class Test: Component
{
	protected override void OnUpdate()
	{
		CameraComponent cameraComponent = GetComponent<CameraComponent>(  );
		cameraComponent.CustomSize = new Vector2( 250f, 250f );
	}
}
