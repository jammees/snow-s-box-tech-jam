using System;

namespace Sandbox.Snow;

public sealed class SnowQualityHandler: Component
{
	[Property] [Group( "Quality" )] private Model HighModel { get; set; }
	[Property] [Group( "Quality" )] private Model MedModel { get; set; }
	[Property] [Group( "Quality" )] private Model LowModel { get; set; }
	
	[Property] [Group( "References" )] private SnowHandler SnowHandler { get; set; }
	
	// [ConVar( "snow_quality" )]
	public static float SnowQuality
	{
		get => _snowQuality;
		set
		{
			bool setValue = true;

			switch ( value )
			{
				case 1:
					Instance.SnowHandler.RenderModel = Instance.LowModel;
					Log.Info( "Set snow quality to: low" );
					break;
				case 2:
					Instance.SnowHandler.RenderModel = Instance.MedModel;
					Log.Info( "Set snow quality to: medium" );
					break;
				case 3:
					Instance.SnowHandler.RenderModel = Instance.HighModel;
					Log.Info( "Set snow quality to: high" );
					break;
				default:
					Log.Warning( $"Invalid snow quality value: {value}! Valid values are 1 to 3 ( low to high )!" );
					setValue = false;
					break;
			}
			
			if ( !setValue ) return;
			_snowQuality = value;
		}
	}
	private static float _snowQuality;
	
	// [ConVar( "snow_texture_size" )]
	public static float SnowTextureSize
	{
		get => _snowTextureSize;
		set
		{
			Log.Info( value );
			
			Log.Warning( "If you are reading this, I forgot to actually implement this lol" );
			_snowTextureSize = (int)value;
			Instance.SnowHandler.TextureSize = _snowTextureSize;
		}
	}
	private static int _snowTextureSize;

	internal static SnowQualityHandler Instance;
	
	protected override void OnStart()
	{
		Instance = this;
	}

	// [ConCmd("snow_reset")]
	private static void ResetSnow()
	{
		Instance.SnowHandler.ResetSnowMask();
	}
}
