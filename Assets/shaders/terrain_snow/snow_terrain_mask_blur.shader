HEADER
{
    Version = 1;
    Description = "Snow terrain mask blur compute";
}

COMMON
{
	#include "common/shared.hlsl"
}


CS
{
	#include "system.fxc"

	Texture2D<float> g_tMask < Attribute( "Mask" ); OutputFormat( R16 ); >;
    RWTexture2D<float> g_tResult < Attribute( "Result" ); OutputFormat( R16 ); >;

	[numthreads( 8, 8, 1 )] 
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
	    float2 size = float2( 1.1, 1.1 ); // 5.0, 5.0

	    float fl2PI = 6.28318530718f;
        float flDirections = 16.0f;
        float flQuality = 4.0f;
        float flTaps = 1.0f;

        // Had to use this because the original function used Sample which can't
        // be used outside of fragment also SampleLevel just screwed everything up
        float vColor = g_tMask[id.xy];

        [unroll]
        for( float d=0.0; d<fl2PI; d+=fl2PI/flDirections)
        {
            [unroll]
            for(float j=1.0/flQuality; j<=1.0; j+=1.0/flQuality)
            {
                flTaps += 1;
                vColor += g_tMask[id.xy + float2( cos(d), sin(d) ) * size * j];    
            }
        }

        g_tResult[id.xy] = vColor / flTaps;
	}	
}
