using System;

namespace Sandbox.Snow;

public sealed class SnowQualityHandler: Component
{
	[Property] [Group( "Quality" )] private Model HighModel { get; set; }
	[Property] [Group( "Quality" )] private Model MedModel { get; set; }
	[Property] [Group( "Quality" )] private Model LowModel { get; set; }
	
	[Property] [Group( "References" )] private ModelRenderer SnowRenderer { get; set; }
	[Property] [Group( "References" )] private SnowHandler SnowHandler { get; set; }
	
	[ConVar( "snow_quality" )]
	public static float SnowQuality
	{
		get => _snowQuality;
		set
		{
			bool setValue = true;

			switch ( value )
			{
				case 1:
					Instance.SnowRenderer.Model = Instance.LowModel;
					Log.Info( "Set snow quality to: low" );
					break;
				case 2:
					Instance.SnowRenderer.Model = Instance.MedModel;
					Log.Info( "Set snow quality to: medium" );
					break;
				case 3:
					Instance.SnowRenderer.Model = Instance.HighModel;
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

	internal static SnowQualityHandler Instance;
	
	protected override void OnStart()
	{
		Instance = this;
	}

	[ConCmd("snow_reset")]
	private static void ResetSnow()
	{
		Instance.SnowHandler.ResetSnowMask();
	}
}
