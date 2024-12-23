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

    // Took and slightly modified from postprocess_standard_p2.shader
    float3 GaussianBlurEx( float3 vColor, float2 vTexCoords )
    {
        float flRemappedBlurSize = flBlurSize;

        const float fl2PI = 6.28318530718f;
        const int   Directions =  1;
        const int   Quality = 1;
        const float tapDivisor =  1.f / ( 1.f + Quality * Directions );
                         
        float angleDelta = fl2PI/float(Directions);
        float sinDelta, cosDelta;
        sincos(angleDelta, sinDelta, cosDelta);
         
        //
        // MDV - got rid of per-iteration trig (cos, sin)  by using trig indentities so we only calculate sin and cos  ONCE -  not every loop iteration!
        //

        float3 cossinD = float3( cosDelta, sinDelta, -sinDelta );
        float2 cossin =  float2( 1, 0 ) * lerp(0.0f, 0.02, flRemappedBlurSize) / float(Quality);
          
        [unroll]
        for( int i =  0; i < Directions; ++i )   // float d=0.0; d<fl2PI; d+=fl2PI/flDirections)
        {  
            float2 d = vTexCoords;
            [unroll] 
            for( int j = 0; j < Quality; ++j)
            {
                d += cossin; 
                vColor += g_tMask[ d ].rgb;  
            }
            cossin = float2( dot( cossin.xy,cossinD.xz),  dot( cossin.yx,cossinD.xy) );    // advance sin and cos  to next sin and cos 
        }
        return vColor * tapDivisor;  
    }

    [numthreads( 8, 8, 1 )] 
    void MainCs( uint3 id : SV_DispatchThreadID )
    {
        float3 finalColor = g_tMask[ id.xy ].rgb;
        finalColor = GaussianBlurEx( finalColor, id.xy );

        g_tResult[id.xy] = finalColor;
    }

// 	[numthreads( 8, 8, 1 )] 
// 	void MainCs( uint3 id : SV_DispatchThreadID )
// 	{
// 	    float2 uv = float2( id.x / 1024.0, id.y / 1024.0 );
// 	    float2 size = float2( 5.0, 5.0 ); // 5.0, 5.0
// 
// 	    float fl2PI = 6.28318530718f;
//         float flDirections = 16.0f;
//         float flQuality = 4.0f;
//         float flTaps = 1.0f;
// 
//         // Had to use this because the original function used Sample which can't
//         // be used outside of fragment also SampleLevel just screwed everything up
//         float3 vColor = g_tMask[id.xy].rgb;
// 
//         [unroll]
//         for( float d=0.0; d<fl2PI; d+=fl2PI/flDirections)
//         {
//             [unroll]
//             for(float j=1.0/flQuality; j<=1.0; j+=1.0/flQuality)
//             {
//                 flTaps += 1;
//                 vColor += g_tMask[id.xy + float2( cos(d), sin(d) ) * size * j].rgb;    
//             }
//         }
// 
//         g_tResult[id.xy] = vColor / flTaps;
// 	}	
}
