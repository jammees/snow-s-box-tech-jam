
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	VrForward();
	Depth(); 
	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsWireframe( "vr_tools_wireframe.shader" );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
	float4 vTintColor : COLOR1;
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;

		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v );
		i.vTintColor = extraShaderData.vTint;

		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );

		return FinalizeVertex( i );
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Mat_Color, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Snow_Color, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( heightmap, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Mat_Normal, Srgb, 8, "None", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Snow_Normal, Srgb, 8, "None", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tMat_Color < Channel( RGBA, Box( Mat_Color ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	Texture2D g_tSnow_Color < Channel( RGBA, Box( Snow_Color ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	Texture2D g_theightmap < Channel( RGBA, Box( heightmap ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tMat_Normal < Channel( RGBA, Box( Mat_Normal ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	Texture2D g_tSnow_Normal < Channel( RGBA, Box( Snow_Normal ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	float g_flheightmap_strength < UiType( Slider ); UiGroup( "Snow,0/,0/0" ); Default1( 15 ); Range1( 0, 15 ); >;
	float g_flheightmap_normal_strength < UiType( Slider ); UiGroup( "Snow,0/,0/0" ); Default1( 15 ); Range1( 0, 15 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init();
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float4 l_0 = Tex2DS( g_tMat_Color, g_sSampler0, i.vTextureCoords.xy );
		float4 l_1 = Tex2DS( g_tSnow_Color, g_sSampler0, i.vTextureCoords.xy );
		float3 l_2 = float3( 0, 0, 1 );
		float l_3 = dot( i.vNormalWs, l_2 );
		float l_4 = saturate( l_3 );
		float4 l_5 = Tex2DS( g_theightmap, g_sSampler0, i.vTextureCoords.xy );
		float l_6 = g_flheightmap_strength;
		float l_7 = l_5.r * l_6;
		float l_8 = l_4 * l_7;
		float l_9 = saturate( l_8 );
		float4 l_10 = saturate( lerp( l_0, l_1, l_9 ) );
		float4 l_11 = Tex2DS( g_tMat_Normal, g_sSampler0, i.vTextureCoords.xy );
		float4 l_12 = Tex2DS( g_tSnow_Normal, g_sSampler0, i.vTextureCoords.xy );
		float l_13 = g_flheightmap_normal_strength;
		float l_14 = l_5.r * l_13;
		float4 l_15 = l_12 * float4( l_14, l_14, l_14, l_14 );
		float4 l_16 = saturate( lerp( l_11, l_15, l_9 ) );
		
		m.Albedo = l_10.xyz;
		m.Opacity = 1;
		m.Normal = l_16.xyz;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );

		// Result node takes normal as tangent space, convert it to world space now
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );

		// for some toolvis shit
		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
        m.TextureCoords = i.vTextureCoords.xy;
		
		return ShadingModelStandard::Shade( i, m );
	}
}
