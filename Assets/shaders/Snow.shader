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
	
//     CreateInputTexture2D( SnowMask, Linear, 8, "None", "", "Snow,1/4", Default( 1.00 ) );
//     Texture2D g_tSnowMask < Channel( R, Box( SnowMask ), Linear );
//                             OutputFormat( BC7 ); >;
                            
//     Texture2D Bindless::GetTexture2D( int nIndex, bool srgb = false );
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
	float4 SobelNormal: NORMAL;
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"
	
	float f_SnowHeight < UiType( Slider ); Default( 5.0 ); Range( 0.0, 25.0 ); UiGroup("Snow,1/1"); >;
	
	CreateInputTexture2D( SnowHeight, Linear, 8, "None", "", "Snow,1/3", Default4( 0.0, 0.0, 0.0, 0.0 ) );
	Texture2D g_tSnowHeight < Channel( RGBA, Box(SnowHeight), Linear ); OutputFormat( BC7 ); >;
	
	Texture2D<float> g_tSnowMask < Attribute("SnowMask"); Default( 1.0 ); > ;
	float g_fUvOffset < Attribute("UvOffset"); >;
	bool g_bNotInPreviewMode < Attribute("NotInPreviewMode"); >;
	
	PixelInput MainVs( VertexInput i )
	{
	    // using the sobel filter we calculate a new normal for
	    // this vertex
	    
	    // calculate uv for 1 pixel
	    float2 centerUv = float2( 1.0 - i.vTexCoord.x, i.vTexCoord.y );
	    
	    float mask0 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x - g_fUvOffset, centerUv.y + g_fUvOffset ), 0 ); // bottom left
	    float mask1 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x              , centerUv.y + g_fUvOffset ), 0 ); // bottom
	    float mask2 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x + g_fUvOffset, centerUv.y + g_fUvOffset ), 0 ); // bottom right
	    float mask3 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x - g_fUvOffset, centerUv.y               ), 0 ); // left
	    float mask4 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x              , centerUv.y               ), 0 ); // center
	    float mask5 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x + g_fUvOffset, centerUv.y               ), 0 ); // right
	    float mask6 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x - g_fUvOffset, centerUv.y - g_fUvOffset ), 0 ); // top left
	    float mask7 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x              , centerUv.y - g_fUvOffset ), 0 ); // top
        float mask8 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x + g_fUvOffset, centerUv.y - g_fUvOffset ), 0 ); // top right
	
	    float4 normal;
	    normal.x = 2.0 * -(mask2-mask0+2*(mask5-mask3)+mask8-mask6); // 10
        normal.y = 2.0 * -(mask6-mask0+2*(mask7-mask1)+mask8-mask2);
        normal.z = 1.0;
        normal.w = 1.0;
        normal = normalize(normal);
	
	    [flatten] if ( g_bNotInPreviewMode )
	    {
            float snowMask = 1.0 - g_tSnowMask.SampleLevel( g_sTrilinearClamp, centerUv, 0 );
            float snowHeight = g_tSnowHeight.SampleLevel( g_sTrilinearClamp, i.vTexCoord, 0 ).r * 3.0 * clamp( snowMask, 0.3, 1.0 );
            i.vPositionOs += float3( 0.0, 0.0, snowHeight );
        
            i.vPositionOs += float3( 0.0, 0.0, f_SnowHeight * snowMask );
	    }
	    else
	    {
	        i.vPositionOs += float3( 0.0, 0.0, f_SnowHeight );
	    }
	
		PixelInput o = ProcessVertex( i );
		o.SobelNormal = normal;
		return FinalizeVertex( o );
	}
}

PS
{
    #include "common/pixel.hlsl"

    float3 blend_udn(float4 n1, float4 n2)
    {
        float3 c = float3(2, 1, 0);
        float3 r;
        r = n2*c.yyz + n1.xyz;
        r =  r*c.xxx -  c.xxy;
        return normalize(r);
    }
    
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::From( i );
		
		// blend normals
        float3 textureNormal = g_tNormal.Sample( g_sTrilinearWrap, float2( 1.0 - i.vTextureCoords.x, i.vTextureCoords.y ) ).rgb;
		m.Normal = blend_udn( float4( textureNormal.r, textureNormal.g, textureNormal.b, 1.0 ), i.SobelNormal );
		
		return ShadingModelStandard::Shade( i, m );
	}
}