// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Stylized_Water"
{
	Properties
	{
		_TRANSPARENCY("TRANSPARENCY", Range( 0 , 1)) = 0
		_BWATER_TXT("BWATER_TXT", 2D) = "white" {}
		_BWATER_COLOR01_V2("BWATER_COLOR01_V2", Color) = (0.2794118,0.8807305,1,0)
		_BWATER_COLOR02_V2("BWATER_COLOR02_V2", Color) = (0.3088235,0.3421906,1,0)
		_Color0("Color 0", Color) = (1,1,1,0)
		_BWATER_COLOR01_V1("BWATER_COLOR01_V1", Color) = (0,0,0,0)
		_BWATER_COLOR02_V1("BWATER_COLOR02_V1", Color) = (0,0,0,0)
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_WAVE01_TXT("WAVE01_TXT", 2D) = "white" {}
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_WAVE02_TXT("WAVE02_TXT", 2D) = "white" {}
		_FOAM("FOAM", Range( 0 , 1)) = 0.9247113
		_WAVE01_COLOR("WAVE01_COLOR", Color) = (0.6551723,0,1,0)
		_WAVE02_COLOR("WAVE02_COLOR", Color) = (1,0,0,0)
		_WAVE01_TILING("WAVE01_TILING", Float) = 1
		_WAVE02_TILING("WAVE02_TILING", Float) = 1
		_WAVE01_SPEED("WAVE01_SPEED", Vector) = (0.2,0.2,0,0)
		_WAVE02_SPEED("WAVE02_SPEED", Vector) = (0.2,0.2,0,0)
		_RIM_COLOR("RIM_COLOR", Color) = (0.2205882,0.3227179,1,0)
		_RIM_SCALE("RIM_SCALE", Float) = 0
		_RIM_POWER("RIM_POWER", Float) = 0
		_RIM_BIAS("RIM_BIAS", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			float4 screenPos;
		};

		uniform float4 _BWATER_COLOR02_V2;
		uniform float4 _BWATER_COLOR01_V2;
		uniform sampler2D _BWATER_TXT;
		uniform float4 _BWATER_TXT_ST;
		uniform float4 _BWATER_COLOR02_V1;
		uniform float4 _BWATER_COLOR01_V1;
		uniform float4 _WAVE01_COLOR;
		uniform sampler2D _WAVE01_TXT;
		uniform float2 _WAVE01_SPEED;
		uniform float _WAVE01_TILING;
		uniform float4 _WAVE02_COLOR;
		uniform sampler2D _WAVE02_TXT;
		uniform float2 _WAVE02_SPEED;
		uniform float _WAVE02_TILING;
		uniform float4 _RIM_COLOR;
		uniform float _RIM_BIAS;
		uniform float _RIM_SCALE;
		uniform float _RIM_POWER;
		uniform float4 _Color0;
		uniform sampler2D _TextureSample1;
		uniform sampler2D _TextureSample0;
		uniform sampler2D _CameraDepthTexture;
		uniform float _FOAM;
		uniform float _TRANSPARENCY;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BWATER_TXT = i.uv_texcoord * _BWATER_TXT_ST.xy + _BWATER_TXT_ST.zw;
			float4 tex2DNode1 = tex2D( _BWATER_TXT, uv_BWATER_TXT );
			float4 lerpResult12 = lerp( _BWATER_COLOR02_V2 , _BWATER_COLOR01_V2 , tex2DNode1.g);
			float4 lerpResult5 = lerp( _BWATER_COLOR02_V1 , _BWATER_COLOR01_V1 , tex2DNode1.r);
			float clampResult17 = clamp( _SinTime.y , 0 , 1 );
			float4 lerpResult14 = lerp( lerpResult12 , lerpResult5 , clampResult17);
			float2 temp_cast_0 = (_WAVE01_TILING).xx;
			float2 uv_TexCoord23 = i.uv_texcoord * temp_cast_0 + float2( 0,0 );
			float2 panner22 = ( uv_TexCoord23 + 1 * _Time.y * _WAVE01_SPEED);
			float4 lerpResult18 = lerp( lerpResult14 , _WAVE01_COLOR , tex2D( _WAVE01_TXT, panner22 ).b);
			float2 temp_cast_1 = (_WAVE02_TILING).xx;
			float2 uv_TexCoord28 = i.uv_texcoord * temp_cast_1 + float2( 0,0 );
			float2 panner29 = ( uv_TexCoord28 + 1 * _Time.y * _WAVE02_SPEED);
			float4 lerpResult30 = lerp( lerpResult18 , _WAVE02_COLOR , tex2D( _WAVE02_TXT, panner29 ).a);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNDotV31 = dot( normalize( ase_worldNormal ), ase_worldViewDir );
			float fresnelNode31 = ( _RIM_BIAS + _RIM_SCALE * pow( 1.0 - fresnelNDotV31, _RIM_POWER ) );
			float clampResult34 = clamp( fresnelNode31 , 0 , 1 );
			float4 lerpResult32 = lerp( lerpResult30 , _RIM_COLOR , clampResult34);
			o.Albedo = lerpResult32.rgb;
			float2 temp_cast_3 = (10.0).xx;
			float2 uv_TexCoord57 = i.uv_texcoord * temp_cast_3 + float2( 0,0 );
			float2 panner58 = ( uv_TexCoord57 + 1 * _Time.y * float2( 0.1,0.1 ));
			float clampResult63 = clamp( tex2D( _TextureSample0, panner58 ).r , 0 , 1 );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth62 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(ase_screenPos))));
			float distanceDepth62 = abs( ( screenDepth62 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _FOAM ) );
			float clampResult68 = clamp( ( clampResult63 * distanceDepth62 ) , 0 , 1 );
			float4 lerpResult70 = lerp( ( _Color0 * tex2D( _TextureSample1, panner58 ).g ) , float4(0.0147059,0,0,0) , clampResult68);
			float4 Emission71 = lerpResult70;
			o.Emission = Emission71.rgb;
			o.Alpha = _TRANSPARENCY;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14501
210;140;1103;759;-603.9986;-2002.564;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;55;-1262.026,2305.922;Float;False;2009.663;867.9782;Comment;14;71;70;69;68;67;66;65;64;63;62;61;60;58;57;Emission;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;73;-1532.778,2492.016;Float;False;Constant;_Float0;Float 0;22;0;Create;True;0;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;57;-1244.026,2485.851;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;45;-1283.542,652.5465;Float;False;1588.776;419.6201;;6;25;23;24;22;20;19;WAVE_2_SPEED;1,1,1,1;0;0
Node;AmplifyShaderEditor.PannerNode;58;-912.333,2540.412;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.1,0.1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;44;-1289.447,1147.376;Float;False;1493.287;412.958;;6;27;26;28;29;21;47;WAVE_1_SPEED;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-1233.542,716.6332;Float;False;Property;_WAVE01_TILING;WAVE01_TILING;14;0;Create;True;0;1;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;13;-1259.976,-840.3318;Float;False;670.8668;545.3494;;3;12;10;11;CHANNEl_G;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;9;-1266.936,-265.6506;Float;False;706.3019;499.2993;;3;6;8;5;CHANNEL_R;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;8;-1216.936,1.448356;Float;False;Property;_BWATER_COLOR01_V1;BWATER_COLOR01_V1;5;0;Create;True;0;0,0,0,0;0.07843138,0.1019608,0.2078432,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-1209.975,-500.9713;Float;False;Property;_BWATER_COLOR01_V2;BWATER_COLOR01_V2;2;0;Create;True;0;0.2794118,0.8807305,1,0;0.1098039,0.4156863,0.3137255,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;23;-911.3381,709.2087;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-1245.266,270.0612;Float;True;Property;_BWATER_TXT;BWATER_TXT;1;0;Create;True;0;None;b6345f9a832f16f42b95a36d6282cedb;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;6;-1215.635,-215.6506;Float;False;Property;_BWATER_COLOR02_V1;BWATER_COLOR02_V1;6;0;Create;True;0;0,0,0,0;0.1647059,0.4509804,0.6235294,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinTimeNode;15;-753.6981,289.5536;Float;True;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;61;-601.1232,2767.94;Float;True;Property;_TextureSample0;Texture Sample 0;9;0;Create;True;0;None;b6345f9a832f16f42b95a36d6282cedb;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;10;-1209.976,-697.4964;Float;False;Property;_BWATER_COLOR02_V2;BWATER_COLOR02_V2;3;0;Create;True;0;0.3088235,0.3421906,1,0;0.1529412,0.5529412,0.4,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;27;-1239.447,1204.801;Float;False;Property;_WAVE02_TILING;WAVE02_TILING;15;0;Create;True;0;1;15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;24;-894.864,911.1666;Float;False;Property;_WAVE01_SPEED;WAVE01_SPEED;16;0;Create;True;0;0.2,0.2;0.05,0.05;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;60;-791.9651,3032.342;Float;False;Property;_FOAM;FOAM;11;0;Create;True;0;0.9247113;0.08;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;28;-917.2451,1197.376;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;5;-825.6339,-19.35125;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;63;-308.9534,2853.387;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;17;-417.7638,-0.2961516;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;12;-854.1083,-790.3319;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;46;-1274.151,1683.152;Float;False;905.4394;517.5123;;6;36;37;33;35;31;34;RIM;1,1,1,1;0;0
Node;AmplifyShaderEditor.DepthFade;62;-462.0766,3033.211;Float;False;True;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;26;-900.769,1399.334;Float;False;Property;_WAVE02_SPEED;WAVE02_SPEED;17;0;Create;True;0;0.2,0.2;-0.1,-0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;22;-540.9313,712.2068;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;14;-158.4413,-325.0013;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;65;-385.8885,2531.982;Float;True;Property;_TextureSample1;Texture Sample 1;7;0;Create;True;0;None;b6345f9a832f16f42b95a36d6282cedb;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;33;-1223.947,1923.84;Float;False;Property;_RIM_BIAS;RIM_BIAS;21;0;Create;True;0;0;3.97;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;66;-339.4006,2355.922;Float;False;Property;_Color0;Color 0;4;0;Create;True;0;1,1,1,0;0.1843137,0.4470589,0.6117647,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;64;-140.6976,2977.966;Float;False;2;2;0;FLOAT;0.075;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-1224.153,2006.051;Float;False;Property;_RIM_SCALE;RIM_SCALE;19;0;Create;True;0;0;-3.02;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;29;-630.5453,1217.496;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;20;-338.2176,692.437;Float;True;Property;_WAVE01_TXT;WAVE01_TXT;8;0;Create;True;0;None;b6345f9a832f16f42b95a36d6282cedb;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;19;-0.4615231,853.9833;Float;False;Property;_WAVE01_COLOR;WAVE01_COLOR;12;0;Create;True;0;0.6551723,0,1,0;0.5529412,0.6627451,0.627451,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;37;-1222.06,2085.664;Float;False;Property;_RIM_POWER;RIM_POWER;20;0;Create;True;0;0;-0.38;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;18;409.7457,736.8709;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;47;-91.52754,1351.732;Float;False;Property;_WAVE02_COLOR;WAVE02_COLOR;13;0;Create;True;0;1,0,0,0;0.654902,0.7843138,0.8352942,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;68;55.72844,2941.201;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;29.30241,2595.724;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;67;-114.1786,2777.618;Float;False;Constant;_Color1;Color 1;9;0;Create;True;0;0.0147059,0,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;21;-421.5485,1189.886;Float;True;Property;_WAVE02_TXT;WAVE02_TXT;10;0;Create;True;0;None;b6345f9a832f16f42b95a36d6282cedb;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;31;-882.3602,1887.713;Float;False;World;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;35;-1218.076,1733.152;Float;False;Property;_RIM_COLOR;RIM_COLOR;18;0;Create;True;0;0.2205882,0.3227179,1,0;0.1568628,0.4588236,0.482353,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;30;595.7064,1218.143;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;34;-537.7172,1886.709;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;70;265.3503,2808.684;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;32;919.268,1805.672;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;74;1044.999,2378.564;Float;False;Property;_TRANSPARENCY;TRANSPARENCY;0;0;Create;True;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;71;504.6346,2874.311;Float;False;Emission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;72;1047.595,2119.979;Float;False;71;0;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1390.433,2025.576;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Custom/Stylized_Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;Back;0;0;False;0;1;False;0;Transparent;0;True;False;0;False;Transparent;;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;4;10;25;False;0.5;False;2;SrcAlpha;OneMinusSrcAlpha;0;Zero;Zero;Add;Add;0;False;0.09;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;57;0;73;0
WireConnection;58;0;57;0
WireConnection;23;0;25;0
WireConnection;61;1;58;0
WireConnection;28;0;27;0
WireConnection;5;0;6;0
WireConnection;5;1;8;0
WireConnection;5;2;1;1
WireConnection;63;0;61;1
WireConnection;17;0;15;2
WireConnection;12;0;10;0
WireConnection;12;1;11;0
WireConnection;12;2;1;2
WireConnection;62;0;60;0
WireConnection;22;0;23;0
WireConnection;22;2;24;0
WireConnection;14;0;12;0
WireConnection;14;1;5;0
WireConnection;14;2;17;0
WireConnection;65;1;58;0
WireConnection;64;0;63;0
WireConnection;64;1;62;0
WireConnection;29;0;28;0
WireConnection;29;2;26;0
WireConnection;20;1;22;0
WireConnection;18;0;14;0
WireConnection;18;1;19;0
WireConnection;18;2;20;3
WireConnection;68;0;64;0
WireConnection;69;0;66;0
WireConnection;69;1;65;2
WireConnection;21;1;29;0
WireConnection;31;1;33;0
WireConnection;31;2;36;0
WireConnection;31;3;37;0
WireConnection;30;0;18;0
WireConnection;30;1;47;0
WireConnection;30;2;21;4
WireConnection;34;0;31;0
WireConnection;70;0;69;0
WireConnection;70;1;67;0
WireConnection;70;2;68;0
WireConnection;32;0;30;0
WireConnection;32;1;35;0
WireConnection;32;2;34;0
WireConnection;71;0;70;0
WireConnection;0;0;32;0
WireConnection;0;2;72;0
WireConnection;0;9;74;0
ASEEND*/
//CHKSM=43E96EE065B3961B91FD9057F08A4193D6B930E2