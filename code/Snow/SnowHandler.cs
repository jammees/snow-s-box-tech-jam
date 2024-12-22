using System;
using System.Net.Http;
using System.Net.Mime;
using Sandbox;
using Sandbox.Rendering;


public sealed class SnowHandler : Component
{
	[Property]
	private ModelRenderer Renderer { get; set; }
	
	[Property]
	private CameraComponent SnowCamera { get; set; }

	// private Texture SnowMask;
	private ComputeShader SnowMaskCompute;
	private ComputeShader SnowMaskBlurCompute;

	private Texture SnowDebugTexture;
	private Texture MaskTexture;
	private Texture SnowRenderTarget;
	private Texture SnowMask;
	private RenderAttributes SnowAttributes = new();

	protected override void OnStart()
	{
		SnowDebugTexture = Texture.Load( FileSystem.Mounted, "textures/debugsnowmask.png" );
		
		// load compute shader
		SnowMaskCompute = new ComputeShader( "shaders/snowmask.shader" );
		SnowMaskBlurCompute = new ComputeShader( "shaders/snowmaskblur.shader" );
		
		// TODO: use compressed formats with 1 channel as this is a lot of vram waste
		// create mask texture
		MaskTexture = Texture.Create( 1024, 1024 )
			.WithUAVBinding()
			.WithFormat( ImageFormat.RGB888 )
			.WithGPUOnlyUsage()
			.Finish();
		
		SnowMask = Texture.Create( 1024, 1024 )
			.WithUAVBinding()
			.WithFormat( ImageFormat.RGB888 )
			.WithGPUOnlyUsage()
			.Finish();
		
		// create render target
		SnowRenderTarget = Texture.CreateRenderTarget( )
			.WithUAVBinding()
			.WithFormat( ImageFormat.RGBA8888 )
			.WithSize( new Vector2( 1024, 1024 ) )
			.WithGPUOnlyUsage()
			.Create();
		
		SnowCamera.RenderTarget = SnowRenderTarget;
		Renderer.MaterialOverride.Set( "SnowMask", SnowMask );
	}

	protected override void OnPreRender()
	{
		SnowAttributes.Set( "Mask", MaskTexture );
		SnowAttributes.Set( "Screen", SnowRenderTarget );
		SnowAttributes.Set( "Result", SnowMask );
		SnowMaskCompute.DispatchWithAttributes( SnowAttributes, 1024, 1024, 1 );
		SnowMaskBlurCompute.DispatchWithAttributes( SnowAttributes, 1024, 1024, 1 );
	}

	public void ResetSnowMask()
	{
		MaskTexture.Dispose();
		MaskTexture = Texture.Create( 1024, 1024 )
			.WithUAVBinding()
			.WithFormat( ImageFormat.RGB888 )
			.WithGPUOnlyUsage()
			.Finish();
	}
}
