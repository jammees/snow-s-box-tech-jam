// Ideally you wouldn't need half these includes for an unlit shader
// But it's stupiod

FEATURES
{
    #include "common/features.hlsl"
}

MODES
{
    VrForward();
    Depth();
    Default();
}

COMMON
{
	#include "common/shared.hlsl"
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		// Add your vertex manipulation functions here
		return FinalizeVertex( o );
	}
}

PS
{
    #include "common/pixel.hlsl"
    
    Texture2DMS<float> g_tDepth < Attribute( "Screen" ); SrgbRead( false ); >;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
	    float2 invertedUv = float2( 1.0 - i.vTextureCoords.x, i.vTextureCoords.y );
	    invertedUv *= 1024.0;
	
		return g_tDepth.Load( uint2( invertedUv ), 0 );
	}
}
