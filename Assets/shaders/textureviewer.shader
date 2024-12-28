HEADER
{
    Version = 1;
    Description = "Snow material";
    DevShader = true;
    DebugInfo = true;
}

FEATURES
{
    #include "common/features.hlsl"
}

COMMON
{
	#include "common/shared.hlsl"
	
	CreateInputTexture2D( DTexture, Linear, 8, "None", "", "Snow,10/20", Default4( 1.00, 1.00, 1.00, 1.00 ) );
    Texture2D g_tDTexture < Channel( RGBA, Box( DTexture ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
    TextureAttribute( g_tDTexture, g_tDTexture ); // for debug purposes
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

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::From( i );
		
        m.Albedo = g_tDTexture.Sample( g_sAniso, i.vTextureCoords ).r;
		
		return ShadingModelStandard::Shade( i, m );
	}
}
