
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
	CreateInputTexture2D( Mat_Normal, Srgb, 8, "NormalizeNormals", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Snow_Normal, Srgb, 8, "NormalizeNormals", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tMat_Color < Channel( RGBA, Box( Mat_Color ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	Texture2D g_tSnow_Color < Channel( RGBA, Box( Snow_Color ), Srgb ); OutputFormat( BC7 ); SrgbRead( True ); >;
	Texture2D g_tMat_Normal < Channel( RGBA, Box( Mat_Normal ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	Texture2D g_tSnow_Normal < Channel( RGBA, Box( Snow_Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	
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
		float3 l_1 = float3( l_0.r, l_0.g, l_0.b );
		float4 l_2 = Tex2DS( g_tSnow_Color, g_sSampler0, i.vTextureCoords.xy );
		float3 l_3 = float3( l_2.r, l_2.g, l_2.b );
		float3 l_4 = float3( 0, 0, 1 );
		float l_5 = dot( i.vNormalWs, l_4 );
		float3 l_6 = saturate( lerp( l_1, l_3, l_5 ) );
		float4 l_7 = Tex2DS( g_tMat_Normal, g_sSampler0, i.vTextureCoords.xy );
		float4 l_8 = Tex2DS( g_tSnow_Normal, g_sSampler0, i.vTextureCoords.xy );
		float4 l_9 = saturate( lerp( l_7, l_8, l_5 ) );
		
		m.Albedo = l_6;
		m.Opacity = 1;
		m.Normal = l_9.xyz;
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
