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

	RWTexture2D<float> g_tMask < Attribute( "Mask" ); OutputFormat( R16 ); >;
    Texture2D<float4> g_tScreen < Attribute( "Screen" ); OutputFormat( RGBA8888 ); SrgbRead( True ); >;

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
//         float screenLum = GetLuminance(g_tScreen[ id.xy ].rgb);
//         float threshold = 0.01;
//         
//         if ( screenLum > threshold )
//         {
//             g_tMask[ id.xy ] = 1.0;
//         }

        float snowDepth = g_tMask[ id.xy ];
        float currDepth = g_tScreen[ id.xy ].r;
        
        if ( currDepth > snowDepth )
        {
            g_tMask[ id.xy ] = currDepth;
        }

//          = clamp(g_tMask[ id.xy ] + g_tScreen[ id.xy ].r, 0.0, 1.0);
	}	
}
