FEATURES
{
    #include "common/features.hlsl"
}

COMMON
{
	#include "common/shared.hlsl"
}

MODES
{
    VrForward();
    Depth();
    ToolVis( S_MODE_TOOLS_VIS );
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
		return FinalizeVertex( o );
	}
}

PS
{
    #include "common/pixel.hlsl"
    
    Texture2D<float3> i_tDebugTexture < Attribute("DTexture"); >;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::From( i );
		
		m.Albedo = m.WorldPosition;
		
		return ShadingModelStandard::Shade( i, m );
	}
}
