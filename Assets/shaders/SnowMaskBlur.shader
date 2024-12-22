HEADER
{
    Version = 1;
    Description = "Snow mask blur compute";
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

	Texture2D<float3> g_tMask < Attribute( "Mask" ); OutputFormat( RGB888 ); SrgbRead( False ); >;
    RWTexture2D<float3> g_tResult < Attribute( "Result" ); OutputFormat( RGB888 ); SrgbRead( False ); >;

	[numthreads( 8, 8, 1 )] 
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
	    float2 uv = float2( id.x / 1024.0, id.y / 1024.0 );
	    float2 size = float2( 5.0, 5.0 ); // 5.0, 5.0
	    
	    float fl2PI = 6.28318530718f;
        float flDirections = 16.0f;
        float flQuality = 4.0f;
        float flTaps = 1.0f;
    
        // Had to use this because the original function used Sample which can't
        // be used outside of fragment also SampleLevel just screwed everything up
        float3 vColor = g_tMask[id.xy].rgb;
    
        [unroll]
        for( float d=0.0; d<fl2PI; d+=fl2PI/flDirections)
        {
            [unroll]
            for(float j=1.0/flQuality; j<=1.0; j+=1.0/flQuality)
            {
                flTaps += 1;
                vColor += g_tMask[id.xy + float2( cos(d), sin(d) ) * size * j].rgb;    
            }
        }
	    
        g_tResult[id.xy] = vColor / flTaps;
	}	
}
