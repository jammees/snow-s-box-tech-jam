HEADER
{
    Version = 1;
    Description = "Snow mask compute";
    DevShader = true;
    DebugInfo = true;
}

COMMON
{
	#include "common/shared.hlsl"
}


CS
{
	#include "system.fxc"
	#include "postprocess/common.hlsl"

	RWTexture2D<float3> g_tMask < Attribute( "Mask" ); OutputFormat( RGB888 ); SrgbRead( False ); >;
    Texture2D<float4> g_tScreen < Attribute( "Screen" ); OutputFormat( RGBA8888 ); SrgbRead( True ); >;

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
// 	    g_tSnowMask[id.xy] = float3( 1.0, 1.0, 0.0 );
//         float4(0.22, 0.10, 0.30, 1.00); background cull color?
        // combines the 2 images together based on the "r" channel
        float screenLum = GetLuminance(g_tScreen[ id.xy ].rgb);
        float threshold = 0.01;
        
        if ( screenLum > threshold )
        {
            g_tMask[ id.xy ] = 1.0;
        }
	}	
}
