// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASESampleShaders/CustomLightingToon"
{
	Properties
	{
		_ToonRamp("Toon Ramp", 2D) = "white" {}
		_Diffuse("Diffuse", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf StandardCustomLighting keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			fixed Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _Diffuse;
		uniform float4 _Diffuse_ST;
		uniform sampler2D _ToonRamp;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#if DIRECTIONAL
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			float2 uv_Diffuse = i.uv_texcoord * _Diffuse_ST.xy + _Diffuse_ST.zw;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			float dotResult3 = dot( ase_worldNormal , ase_worldlightDir );
			float2 temp_cast_0 = (saturate( (dotResult3*0.5 + 0.5) )).xx;
			float4 temp_output_42_0 = ( tex2D( _Diffuse, uv_Diffuse ) * tex2D( _ToonRamp, temp_cast_0 ) );
			UnityGI gi11 = gi;
			float3 diffNorm11 = ase_worldNormal;
			gi11 = UnityGI_Base( data, 1, diffNorm11 );
			float3 indirectDiffuse11 = gi11.indirect.diffuse + diffNorm11 * 0.0001;
			c.rgb = ( temp_output_42_0 * ( _LightColor0 * float4( ( indirectDiffuse11 + ase_lightAtten ) , 0.0 ) ) ).rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14501
1902;11;1906;1044;1138.396;655.8897;1.662337;True;False
Node;AmplifyShaderEditor.CommentaryNode;48;-2016,32;Float;False;540.401;320.6003;Comment;3;1;3;2;N . L;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;2;-1952,240;Float;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;51;-1143.099,-115.6995;Float;False;723.599;290;Also know as Lambert Wrap or Half Lambert;3;5;4;15;Diffuse Wrap;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;1;-1904,80;Float;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;5;-1093.099,59.30051;Float;False;Constant;_WrapperValue;Wrapper Value;0;0;Create;True;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;3;-1616,144;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;52;-880,320;Float;False;812;304;Comment;5;7;8;11;10;12;Attenuation and Ambient;1,1,1,1;0;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;4;-827.6974,-65.69949;Float;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;7;-809.6001,504;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;11;-624,448;Float;False;Tangent;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;15;-594.4999,-58.89988;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;12;-384,480;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LightColorNode;8;-768,368;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;43;-224,-304;Float;True;Property;_Diffuse;Diffuse;1;0;Create;True;0;c3cc487798e943a4dab0806f1ea15c21;c3cc487798e943a4dab0806f1ea15c21;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;6;-224,-80;Float;True;Property;_ToonRamp;Toon Ramp;0;0;Create;True;0;52e66a9243cdfed44b5e906f5910d35b;5618466a35a0d784188f3bb05731d91c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-224,368;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;161,-189;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;60;-1045.854,982.0867;Float;False;1932.433;558.8087;Comment;7;56;57;55;53;59;58;54;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;58;-130.798,1032.087;Float;False;Property;_Color0;Color 0;3;0;Create;True;0;1,0,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;485.6002,153.2;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;61;537.8446,-199.2167;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-992.8537,1235.895;Float;False;Property;_Bias;Bias;5;0;Create;True;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-993.8537,1328.895;Float;False;Property;_Scale;Scale;4;0;Create;True;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-995.8537,1425.895;Float;False;Property;_Power;Power;2;0;Create;True;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;62;197.8446,-479.2167;Float;False;Property;_Color1;Color 1;6;0;Create;True;0;1,0,0,0;0.7573529,0.194907,0.194907,0.497;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;59;78.86267,1275.859;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;53;-675.7452,1280.249;Float;False;World;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;54;621.5793,1066.959;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1290.41,358.3659;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;ASESampleShaders/CustomLightingToon;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;4;10;25;False;0.5;False;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0.03;0,0,0,0;VertexScale;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
WireConnection;4;0;3;0
WireConnection;4;1;5;0
WireConnection;4;2;5;0
WireConnection;15;0;4;0
WireConnection;12;0;11;0
WireConnection;12;1;7;0
WireConnection;6;1;15;0
WireConnection;10;0;8;0
WireConnection;10;1;12;0
WireConnection;42;0;43;0
WireConnection;42;1;6;0
WireConnection;23;0;42;0
WireConnection;23;1;10;0
WireConnection;61;0;62;0
WireConnection;61;1;42;0
WireConnection;59;0;53;0
WireConnection;53;1;55;0
WireConnection;53;2;56;0
WireConnection;53;3;57;0
WireConnection;54;1;58;0
WireConnection;54;2;59;0
WireConnection;0;13;23;0
ASEEND*/
//CHKSM=5C8E0C9FFAA851F7ECF91562452C3CCAF949C830