using System;
using System.Net.Http;
using System.Net.Mime;
using Sandbox;
using Sandbox.Rendering;


public sealed class SnowHandler : Component
{
	[Property]
	public Model RenderModel { get; set; }
	
	[Property]
	private CameraComponent SnowCamera { get; set; }

	[Property]
	public int TextureSize { get; set; } = 1024;
	
	private SceneCustomObject SnowObject;

	private ComputeShader SnowMaskCompute;
	private ComputeShader SnowMaskBlurCompute;

	private Texture MaskTexture;
	private Texture SnowRenderTarget;
	private Texture SnowMask;
	private readonly RenderAttributes SnowAttributes = new();
	
	protected override void DrawGizmos()
	{
		Gizmo.Draw.Model( RenderModel );
	}

	protected override void OnStart()
	{
		// Renderer.MaterialOverride = Renderer.MaterialOverride.CreateCopy();
		
		// load compute shader
		SnowMaskCompute = new ComputeShader( "shaders/plane_snow/snowmask.shader" );
		SnowMaskBlurCompute = new ComputeShader( "shaders/plane_snow/snowmaskblur.shader" );
		
		SnowCamera.AddHookAfterTransparent( "SnowGetCameraDepth", 0, camera =>
		{
			Graphics.GrabDepthTexture( "Screen", SnowAttributes );
		} );
	}

	protected override void OnEnabled()
	{
		SnowCamera.Enabled = true;
		CreateTextures();
		SnowObject = new( Scene.SceneWorld );
		SnowObject.RenderOverride = OnSnowRender;
		SnowObject.Attributes.Set( "SnowMask", SnowMask );
		SnowObject.Attributes.Set( "UvOffset", 1f / TextureSize );
		SnowObject.Attributes.Set( "NotInPreviewMode", true );
	}
	
	protected override void OnDisabled()
	{
		MaskTexture?.Dispose();
		SnowMask?.Dispose();
		SnowRenderTarget?.Dispose();
		SnowCamera.Enabled = false;
		SnowObject.Delete();
	}

	protected override void OnPreRender()
	{
		SnowAttributes.Set( "Mask", MaskTexture );
		// SnowAttributes.Set( "Screen", SnowRenderTarget );
		SnowAttributes.Set( "Result", SnowMask );
		SnowMaskCompute.DispatchWithAttributes( SnowAttributes, TextureSize, TextureSize, 1 );
		SnowMaskBlurCompute.DispatchWithAttributes( SnowAttributes, TextureSize, TextureSize, 1 );
	}

	private void CreateTextures()
	{
		// TODO: use compressed formats with 1 channel as this is a lot of vram waste
		// create mask texture
		MaskTexture = Texture.Create( TextureSize, TextureSize )
			.WithUAVBinding()
			.WithFormat( ImageFormat.R16 )
			.WithGPUOnlyUsage()
			.WithDynamicUsage()
			.Finish();
		
		SnowMask = Texture.Create( TextureSize, TextureSize )
			.WithUAVBinding()
			.WithFormat( ImageFormat.R16 )
			.WithGPUOnlyUsage()
			.WithDynamicUsage()
			.Finish();
		
		// create render target
		SnowRenderTarget = Texture.CreateRenderTarget( )
			.WithUAVBinding()
			.WithFormat( ImageFormat.RGBA8888 )
			.WithSize( new Vector2( TextureSize, TextureSize ) )
			.WithGPUOnlyUsage()
			.Create();
		
		SnowCamera.RenderTarget = SnowRenderTarget;
	}
	
	private void OnSnowRender( SceneObject obj )
	{
		Graphics.DrawModel( RenderModel, WorldTransform, SnowObject.Attributes );
	}
	
	public void ResetSnowMask()
	{
		MaskTexture.Dispose();
		MaskTexture = Texture.Create( TextureSize, TextureSize )
			.WithUAVBinding()
			.WithFormat( ImageFormat.R16 )
			.WithGPUOnlyUsage()
			.Finish();
	}
}
