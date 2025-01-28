using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Rendering;

namespace Sandbox.Snow.Terrain;

public sealed class SnowTerrainHandler : Component
{
	[Property]
	[RequireComponent]
	private Sandbox.Terrain Terrain { get; set; }

	[Property]
	[Group("Compute Passes")]
	private Shader FirstPassShader { get; set; }
	
	[Property]
	[Group("Compute Passes")]
	private Shader SecondPassShader { get; set; }

	[Property]
	[Group( "Configuration" )]
	private float SnowHeight
	{
		get => _snowHeight;
		set
		{
			_snowHeight = value.Clamp( 0f, 100f );
			
			if ( !SnowCamera.IsValid() ) return;
			
			UpdateCamera();
			UpdateAttributes();
		}
	}

	private float _snowHeight;
	
	[Property]
	[Group("Configuration")]
	private TagSet RenderTags { get; set; }
	
	private int TextureSize => Terrain.Storage.Resolution;
	
	private Texture MaskTexture;
	private Texture SnowRenderTarget;
	private Texture SnowMask;
	private Texture TerrainHeight;
	private Texture TerrainControl;
	private readonly RenderAttributes SnowAttributes = new();
	
	private CameraComponent SnowCamera;
	private ComputeShader MaskComputeFirstPass;
	private ComputeShader MaskComputeSecondPass;

	private IDisposable RenderHook;

	/// <summary>
	/// Disallows the OnUpdate to run
	/// if it is the first time it was called. Reason is
	/// for some reason the missing texture appears in the corner
	/// of the terrain. Maybe because it didn't finish transferring
	/// to the gpu?
	/// </summary>
	private bool FirstTime = true;

	[StructLayout( LayoutKind.Sequential, Pack = 0 )]
	private struct SnowTerrain
	{
		public int SnowMaskIndex;
		public int SnowDebugMode;
		public float SnowHeight;
		public float Padding1;
	}
	
	private GpuBuffer<SnowTerrain> SnowTerrainBuffer;

	#region Debug Commands
	// REMOVE THESE IF UNWANTED
	private static SnowTerrainHandler Instance;

	[ConVar("snow_terrain_use_raw_mask")]
	private static bool UseRawMask
	{
		get
		{
			if ( Instance is null )	return false;

			return Instance._useRawMask;
		}
		set
		{
			if ( Instance is null ) return;
			
			Instance._useRawMask = value;
		}
	}
	private bool _useRawMask;
	
	[ConVar("snow_terrain_pause")]
	private static bool TerrainPause
	{
		get
		{
			if ( Instance is null )	return false;

			return Instance._terrainPause;
		}
		set
		{
			if ( Instance is null ) return;
			
			Instance._terrainPause = value;
		}
	}
	private bool _terrainPause;

	// [ConVar( "snow_terrain_height" )]
	// private static float SnowTerrainHeight
	// {
	// 	get => Instance.SnowHeight;
	// 	set => Instance.SnowHeight = value;
	// }
	
	[ConVar("snow_terrain_debug", Min = 0, Max = 1, Help = "Debug mode")]
	[DefaultValue(false)]
	private static int DebugMode
	{
		get
		{
			if ( Instance is null )	return 0;

			return Instance._debugMode;
		}
		set
		{
			if ( Instance is null ) return;
			
			Instance._debugMode = value;
		}
	}

	private int _debugMode;

	[ConCmd("snow_terrain_reset")]
	private static void ResetSnow()
	{
		if ( Instance is null ) return;
		
		Instance.CreateTextures();		
	}
	#endregion

	protected override void OnStart()
	{
		if ( IsProxy )
			return;
		
		Instance = this;
		
		MaskComputeFirstPass = new ComputeShader( FirstPassShader.ResourcePath );
		MaskComputeSecondPass = new ComputeShader( SecondPassShader.ResourcePath );
	}

	protected override void OnEnabled()
	{
		if ( IsProxy )
			return;
		
		// create camera
		var gameObject = new GameObject( GameObject, true, "TerrainCamera" );
		SnowCamera = gameObject.AddComponent<CameraComponent>();
		SnowCamera.IsMainCamera = false;
		SnowCamera.Orthographic = true;
		SnowCamera.ZNear = 0;
		SnowCamera.OrthographicHeight = Terrain.Storage.TerrainSize;
		SnowCamera.LocalRotation = Rotation.From( new Angles( -90f, 90f, 0f ) );
		SnowCamera.BackgroundColor = Color.Black;
		SnowCamera.RenderTags = RenderTags;
		UpdateCamera();
		
		CreateTextures();
		
		// renderhook
		// please Garry do not remove this, or at least allow CommandLists to
		// grab the depth too
		RenderHook = SnowCamera.AddHookAfterTransparent( "SnowGetCameraDepth", 0, _ =>
		{
			Graphics.GrabDepthTexture( "Depth", SnowAttributes );
		} );
	}

	protected override void OnDisabled()
	{
		if ( IsProxy )
			return;
		
		DisposeTextures();
		RenderHook?.Dispose();
		SnowCamera?.DestroyGameObject();
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;
		
		if ( SnowTerrainBuffer is null )
			SnowTerrainBuffer = new(1);
		
		Assert.NotNull( MaskTexture );
		Assert.NotNull( SnowMask );
		Assert.NotNull( TerrainHeight );

		if ( _debugMode is 1 )
		{
			DebugOverlay.ScreenText(
				new Vector2( 50f ),
				text: "TERRAIN DEBUG MODE",
				flags: TextFlag.Left );
		}

		// Somebody please explain this that why does the
		// missing texture appear in the corner if I don't wait
		// even a single frame?? Doesn't make much sense to me
		if ( FirstTime )
		{
			FirstTime = false;
			return;
		}

		if ( TerrainPause )
			return;
		
		MaskComputeFirstPass.DispatchWithAttributes( SnowAttributes, TextureSize, TextureSize, 1 );
		if ( _useRawMask is false )
			MaskComputeSecondPass.DispatchWithAttributes( SnowAttributes, TextureSize, TextureSize, 1 );

		SnowTerrain snowTerrain = new()
		{
			SnowMaskIndex = _useRawMask is false ? SnowMask.Index : MaskTexture.Index,
			SnowHeight = SnowHeight,
			SnowDebugMode = _debugMode
		};

		SnowTerrainBuffer.SetData( new List<SnowTerrain>() { snowTerrain } );
		
		Scene.RenderAttributes.Set( "SnowTerrain", SnowTerrainBuffer );
	}

	private void UpdateCamera()
	{
		if ( !SnowCamera.IsValid() ) return;

		SnowCamera.LocalPosition = new Vector3( Terrain.Storage.TerrainSize * 0.5f ).WithZ( -SnowHeight );
		SnowCamera.ZFar = Terrain.Storage.TerrainHeight + SnowHeight * 2;

		// SnowCamera.LocalPosition = new Vector3( Terrain.Storage.TerrainSize * 0.5f ).WithZ( 0 );
		// SnowCamera.ZFar = Terrain.Storage.TerrainHeight;
	}
	
	private void UpdateAttributes()
	{
		SnowAttributes.Set( "Mask", MaskTexture );
		SnowAttributes.Set( "Result", SnowMask );
		SnowAttributes.Set( "Heightmap", TerrainHeight );
		SnowAttributes.Set( "Controlmap", TerrainControl );
		SnowAttributes.Set( "SnowHeight", SnowHeight );
		SnowAttributes.Set( "TerrainHeight", Terrain.Storage.TerrainHeight );
	}
	
	private void DisposeTextures()
	{
		MaskTexture?.Dispose();
		SnowMask?.Dispose();
		SnowRenderTarget?.Dispose();
		TerrainHeight?.Dispose();
		TerrainControl?.Dispose();
	}
	
	private void CreateTextures()
	{
		DisposeTextures();
		
		MaskTexture = Texture.Create( TextureSize, TextureSize, ImageFormat.R16 )
			.WithUAVBinding()
			.WithName( "snow_texture" )
			.Finish();
		
		SnowMask = Texture.Create( TextureSize, TextureSize, ImageFormat.R16 )
			.WithUAVBinding()
			.WithName( "snow_mask" )
			.Finish();
		
		TerrainHeight = Texture.Create( TextureSize, TextureSize, ImageFormat.R16 )
			.WithData( new ReadOnlySpan<ushort>( Terrain.Storage.HeightMap ) )
			.WithUAVBinding()
			.WithName( "terrain_height" )
			.Finish();
		
		TerrainControl = Texture.Create( TextureSize, TextureSize )
			.WithData( new ReadOnlySpan<Color32>( Terrain.Storage.ControlMap ) )
			.WithUAVBinding()
			.WithName( "terrain_control" )
			.Finish();
		
		// create render target
		SnowRenderTarget = Texture.CreateRenderTarget( )
			.WithFormat( ImageFormat.RGBA8888 )
			.WithSize( new Vector2( TextureSize, TextureSize ) )
			.Create();

		UpdateAttributes();

		SnowCamera.RenderTarget = SnowRenderTarget;
	}
}
