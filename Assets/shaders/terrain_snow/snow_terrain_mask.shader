HEADER
{
    Version = 1;
    Description = "Snow mask compute";
}

COMMON
{
	#include "common/shared.hlsl"
}


CS
{
	#include "system.fxc"

    // destination
	RWTexture2D<float4> g_tMask < Attribute( "Mask" ); Filter( Point ); AddressU( Clamp ); AddressV( Clamp ); >;

    // controls
    Texture2DMS<float> g_tDepth < Attribute( "Depth" ); Filter( Point ); AddressU( Clamp ); AddressV( Clamp ); >;
    Texture2D<float> g_tHeightmap < Attribute( "Heightmap" ); Filter( Point ); AddressU( Clamp ); AddressV( Clamp ); >;
    Texture2D<float4> g_tControlmap < Attribute( "Controlmap" ); Filter( Point ); AddressU( Clamp ); AddressV( Clamp ); >;

    // other random stuffs
    float g_fSnowHeight < Attribute("SnowHeight"); >;
    float g_fTerrainHeight < Attribute("TerrainHeight"); >;
//     float g_fCameraFarZ < Attribute("CameraFarZ"); >;

// #1 attempt
// 	[numthreads( 8, 8, 1 )]
// 	void MainCs( uint3 id : SV_DispatchThreadID )
// 	{
//         float maskValue = g_tMask[id.xy];
//         float depthValue = g_tDepth[id.xy];
//         float heightValue = g_tHeightmap[id.xy];
// 
//         // create control mask
//         // this assumes that the snow texture is the first one in the list
//         float4 ctrl = g_tControlmap[id.xy];
//         float ctrlMask = 1.0 - saturate(
//             ctrl.g > 0.5 ? 1.0 : 0.0
//             + ctrl.b > 0.5 ? 1.0 : 0.0
//             + ctrl.a > 0.5 ? 1.0 : 0.0);
// 
//         float surfaceHeight = heightValue + 0.001;//g_fSnowHeight; //0.00002 * 160//240.0; // x8
//         float penetration = surfaceHeight - ( 1.0 - depthValue );
// 
//         float storedValue = max( saturate( penetration * 400.0 * ctrlMask ), maskValue );
// 
//         g_tMask[id.xy] = storedValue//ctrlMask > 0.5 ? storedValue : 1.0;
// 	}	

// #2 attemp
// 	[numthreads( 8, 8, 1 )]
// 	void MainCs( uint3 id : SV_DispatchThreadID )
// 	{
//         float depth = 1.0 - g_tDepth[ id.xy ];
//         depth = RemapValClamped(depth, 0.0, 1.0, 0, g_fCameraFarZ);
//         
//         float height = g_tHeightmap[id.xy];
//         
//         // dear god
//         // anyways if anyone is reading this, Hi!
//         // Please do not mind this abomination that somehow, someway
//         // makes the objects look like that they are actually penetrate
//         // the snow
//         // Also this effect breaks if the heightmap is 0. You're welcome
//         depth += height > 0.5 ? g_fSnowHeight : height < 0.5 ? -g_fSnowHeight*0.5 : 0;
//         
//         height *= g_fTerrainHeight;
//         
//         float snowSurfaceHeight = height + g_fSnowHeight;
//         
//         float penetration = snowSurfaceHeight - depth;
//         
//         g_tMask[id.xy] = max( penetration, g_tMask[id.xy] );
// 	}

    // This works perfectly as long as the ratio between the terrain height and the
    // snow height is 100:1
    // I tried a lot of things to fix this however I just couldn't
    // if anyone is reading this and has a solution to this, please tell me :)
    [numthreads( 8, 8, 1 )]
    void MainCs( uint3 id : SV_DispatchThreadID )
    {
        float depth = 1.0 - g_tDepth.Load( id.xy, 0 );
        depth = RemapValClamped(depth, 0.0, 1.0, 0, g_fTerrainHeight + g_fSnowHeight * 2);
        
        float height = g_tHeightmap[id.xy];

        // correction
        float graphMultiplier = clamp( 2.0 * ( height - 0.5 ), -1.0, 1.0 );
        depth += g_fSnowHeight * graphMultiplier;

        height *= g_fTerrainHeight;

        float snowSurfaceHeight = height + g_fSnowHeight;
        float penetration = snowSurfaceHeight - depth;
        penetration = RemapValClamped( penetration, 0.0, g_fSnowHeight, 0.0, 1.0 );

        float4 ctrl = g_tControlmap[id.xy];
        float ctrlMask = saturate(
            ctrl.g > 0.5 ? 1.0 : 0.0
            + ctrl.b > 0.5 ? 1.0 : 0.0
            + ctrl.a > 0.5 ? 1.0 : 0.0);

        penetration = max( penetration, ctrlMask );

        g_tMask[id.xy] = max( penetration, g_tMask[id.xy] );
    }
}
