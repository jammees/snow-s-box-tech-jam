using Sandbox;

public sealed class PreventOob : Component
{
	private Transform OriginTransform;
	
	protected override void OnStart()
	{
		if ( IsProxy )
			return;
		
		OriginTransform = WorldTransform;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;
		
		if ( WorldPosition.z > -500f ) return;

		WorldTransform = OriginTransform;
		
		// reset angular- and velocity
		bool hasRigidbody = Components.TryGet<Rigidbody>( out var rigidbody );
		if ( hasRigidbody )
		{
			rigidbody.AngularVelocity = default;
			rigidbody.Velocity = default;
		}
		
		bool hasModelPhysics = Components.TryGet<ModelPhysics>( out var modelPhysics );
		if ( hasModelPhysics )
		{
			ResetModelForces( modelPhysics );
		}
	}

	private async void ResetModelForces( ModelPhysics modelPhysics )
	{
		modelPhysics.MotionEnabled = false;
		modelPhysics.Enabled = false;
		await GameTask.DelayRealtime( 1 );
		modelPhysics.Enabled = true;
		modelPhysics.MotionEnabled = true;
	}
}
