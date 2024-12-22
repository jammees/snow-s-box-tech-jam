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
	
	CreateInputTexture2D( SnowMask, Linear, 8, "None", "", "Snow,10/20", Default4( 1.00, 1.00, 1.00, 1.00 ) );
    Texture2D g_tSnowMask < Channel( RGBA, Box( SnowMask ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
    TextureAttribute( g_tSnowMask, g_tSnowMask ); // for debug purposes
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
	float4 SobelNormal: NORMAL;
};

VS
{
	#include "common/vertex.hlsl"
	
	float f_SnowHeight < UiType( Slider ); Default( 5.0 ); Range( 0.0, 25.0 ); UiGroup("Snow,10/20"); >;
	FloatAttribute( f_SnowHeight, f_SnowHeight );
	
	// Mask to multiply the height with
    // should be 64x64 -> basically the amount of vertices on one side :)
    
    
    
	PixelInput MainVs( VertexInput i )
	{
	    // using the sobel filter we calculate a new normal for
	    // this vertex
	    
	    // calculate uv for 1 pixel
	    float uvOffset = ( 1.0 / 1024.0 ); //* 5.0;
	    float2 centerUv = float2( 1.0 - i.vTexCoord.x, i.vTexCoord.y );
	    
	    // oh boy it is time to sample
	    // h stants for height too lazy to not abbreviate it
	    // [6][7][8]
        // [3][4][5]
        // [0][1][2]
        // vec3 n;
        // n.x = scale * -(s[2]-s[0]+2*(s[5]-s[3])+s[8]-s[6]);
        // n.y = scale * -(s[6]-s[0]+2*(s[7]-s[1])+s[8]-s[2]);
        // n.z = 1.0;
        // n = normalize(n);
	    float mask0 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x - uvOffset, centerUv.y + uvOffset ), 0 ).r; // bottom left
	    float mask1 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x           , centerUv.y + uvOffset ), 0 ).r; // bottom
	    float mask2 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x + uvOffset, centerUv.y + uvOffset ), 0 ).r; // bottom right
	    float mask3 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x - uvOffset, centerUv.y            ), 0 ).r; // left
	    float mask4 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x           , centerUv.y            ), 0 ).r; // center
	    float mask5 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x + uvOffset, centerUv.y            ), 0 ).r; // right
	    float mask6 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x - uvOffset, centerUv.y - uvOffset ), 0 ).r; // top left
	    float mask7 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x           , centerUv.y - uvOffset ), 0 ).r; // top
        float mask8 = g_tSnowMask.SampleLevel( g_sTrilinearClamp, float2( centerUv.x + uvOffset, centerUv.y - uvOffset ), 0 ).r; // top right
	
	    float4 normal;
	    normal.x = 2.0 * -(mask2-mask0+2*(mask5-mask3)+mask8-mask6); // 10
        normal.y = 2.0 * -(mask6-mask0+2*(mask7-mask1)+mask8-mask2);
        normal.z = 1.0;
        normal.w = 1.0;
        normal = normalize(normal);
	
// 	    float mask = clamp(1.0 - g_tSnowMask.SampleLevel( g_sTrilinearWrap, float2( 1.0 - i.vTexCoord.x, i.vTexCoord.y ), 0 ).r, 0.0, 1.0); 
        i.vPositionOs += float3( 0.0, 0.0, f_SnowHeight * ( 1.0 - mask4 ) );
	
		PixelInput o = ProcessVertex( i );
		o.SobelNormal = normal;
		return FinalizeVertex( o );
	}
}

PS
{
    #include "common/pixel.hlsl"

    // Mask to multiply the height with
    // should be 64x64 -> basically the amount of vertices on one side :)
//     CreateInputTexture2D( SnowMask, Linear, 8, "None", "_color", "Snow,20/20", Default4( 1.00, 1.00, 1.00, 1.00 ) );
//     Texture2D g_tSnowMask < Channel( RGBA, Box( SnowMask ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
//     TextureAttribute( g_tSnowMask, g_tSnowMask );
    
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
		
//         m.Albedo = g_tSnowMask.Sample( g_sAniso, float2( 1.0 - i.vTextureCoords.x, i.vTextureCoords.y ) ).rgb;
		
		// blend normals
        float3 textureNormal = g_tNormal.Sample( g_sTrilinearWrap, float2( 1.0 - i.vTextureCoords.x, i.vTextureCoords.y ) ).rgb;
		m.Normal = blend_udn( float4( textureNormal.r, textureNormal.g, textureNormal.b, 1.0 ), i.SobelNormal );
// 		m.Normal = i.SobelNormal.rgb;
		
		return ShadingModelStandard::Shade( i, m );
	}
}
