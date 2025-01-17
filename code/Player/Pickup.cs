using System;
using Sandbox;
using Sandbox.Diagnostics;

public sealed class Pickup : Component
{
	[Property]
	[RequireComponent]
	private PlayerController Player { get; set; }

	private PhysicsBody _heldObject;

	public PhysicsBody HeldObject
	{
		get => _heldObject;
		set
		{
			if ( value is null )
			{
				bool hasRenderer = HeldObject.GetGameObject().Components.TryGet<ModelRenderer>( out var renderer );
				if ( !hasRenderer ) return;
				renderer.RenderType = ModelRenderer.ShadowRenderType.On;
			}
			else
			{
				bool hasRenderer = value.GetGameObject().Components.TryGet<ModelRenderer>( out var renderer );
				if ( !hasRenderer ) return;
				renderer.RenderType = ModelRenderer.ShadowRenderType.Off;
			}

			_heldObject = value;
		}
	}

	public PhysicsBody LookedObject;
	
	private Transform PositionOffset;
	private bool ObjectLocked;
	
	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;
		
		if ( HeldObject.IsValid() )
		{
			if ( Input.Down( "attack2" ) )
			{
				HeldObject.MotionEnabled = false;

				HeldObject = null;
				PositionOffset = default;
				ObjectLocked = true;
				return;
			}
			
			if ( !Input.Down( "attack1" ) )
			{
				HeldObject = null;
				PositionOffset = default;
			}
			else
			{
				return;
			}
		}

		var hitObject = Scene.Trace.Ray( Scene.Camera.ScreenNormalToRay( 0.5f ), 800f )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithTag( "snowcollider" )
			.Run();

		LookedObject = null;
		
		if ( !hitObject.Hit || hitObject.GameObject is null ) return;

		if ( hitObject.Body.BodyType == PhysicsBodyType.Static ) return;

		LookedObject = hitObject.Body;
		
		if ( Input.Down( "attack1" ) && !ObjectLocked )
		{
			HeldObject = hitObject.Body;
			HeldObject.MotionEnabled = true;
			PositionOffset = Scene.Camera.WorldTransform.ToLocal( HeldObject.Transform );
		};

		if ( !Input.Down( "attack1" ) )
		{
			ObjectLocked = false;
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;
		
		if ( !HeldObject.IsValid() || !Input.Down( "attack1" ) ) return;
		
		HeldObject.SmoothMove( Scene.Camera.WorldTransform.ToWorld( PositionOffset ), 0.5f * Time.Delta, Time.Delta );
	}
}
