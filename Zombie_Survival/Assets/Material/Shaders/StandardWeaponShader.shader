// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "HQ FPS Weapons/Standard Weapon"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_ColorTint("Color Tint", Color) = (1,1,1,1)
		_ColorOpacity("Color&Opacity", 2D) = "white" {}
		[Normal]_NormalMap("Normal Map", 2D) = "bump" {}
		_MaskMap("Mask Map", 2D) = "white" {}
		_AOIntensityReversed("AO Intensity (Reversed)", Range( 0 , 1)) = 0
		_SmoothnessIntensity("Smoothness Intensity", Range( 0 , 1)) = 1
		_Emmision("Emmision", 2D) = "black" {}
		[HDR]_EmmisionMultiplier("Emmision Multiplier", Color) = (0,0,0,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform sampler2D _ColorOpacity;
		uniform float4 _ColorOpacity_ST;
		uniform float4 _ColorTint;
		uniform sampler2D _Emmision;
		uniform float4 _Emmision_ST;
		uniform float4 _EmmisionMultiplier;
		uniform sampler2D _MaskMap;
		uniform float4 _MaskMap_ST;
		uniform float _SmoothnessIntensity;
		uniform float _AOIntensityReversed;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			o.Normal = UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) );
			float2 uv_ColorOpacity = i.uv_texcoord * _ColorOpacity_ST.xy + _ColorOpacity_ST.zw;
			float4 tex2DNode23 = tex2D( _ColorOpacity, uv_ColorOpacity );
			float4 appendResult24 = (float4(tex2DNode23.r , tex2DNode23.g , tex2DNode23.b , 0.0));
			float4 appendResult25 = (float4(_ColorTint.r , _ColorTint.g , _ColorTint.b , 0.0));
			o.Albedo = ( appendResult24 * appendResult25 ).xyz;
			float2 uv_Emmision = i.uv_texcoord * _Emmision_ST.xy + _Emmision_ST.zw;
			o.Emission = ( tex2D( _Emmision, uv_Emmision ) * _EmmisionMultiplier ).rgb;
			float2 uv_MaskMap = i.uv_texcoord * _MaskMap_ST.xy + _MaskMap_ST.zw;
			float4 tex2DNode4 = tex2D( _MaskMap, uv_MaskMap );
			o.Metallic = saturate( tex2DNode4.r );
			o.Smoothness = ( saturate( tex2DNode4.a ) * _SmoothnessIntensity );
			o.Occlusion = ( saturate( tex2DNode4.g ) + _AOIntensityReversed );
			o.Alpha = 1;
			clip( ( tex2DNode23.a * _ColorTint.a ) - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15800
0;278;1524;527;2769.594;398.947;2.414829;True;True
Node;AmplifyShaderEditor.SamplerNode;4;-991.3577,274.624;Float;True;Property;_MaskMap;Mask Map;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;23;-1205.174,-320.7561;Float;True;Property;_ColorOpacity;Color&Opacity;2;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;22;-1186.565,-104.9521;Float;False;Property;_ColorTint;Color Tint;1;0;Create;True;0;0;False;0;1,1,1,1;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;17;-659.9036,489.6279;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-905.2424,560.5341;Float;False;Property;_SmoothnessIntensity;Smoothness Intensity;6;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-605.3872,390.1621;Float;False;Property;_AOIntensityReversed;AO Intensity (Reversed);5;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-545.4167,545.5414;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;25;-959.8218,-106.2932;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;15;-564.4962,300.4108;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;19;-551.397,-567.1044;Float;True;Property;_Emmision;Emmision;7;0;Create;True;0;0;False;0;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;24;-856.683,-297.8448;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;21;-510.0837,-370.8497;Float;False;Property;_EmmisionMultiplier;Emmision Multiplier;8;1;[HDR];Create;True;0;0;False;0;0,0,0,1;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;18;-294.6037,295.928;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-548.1431,11.25476;Float;True;Property;_NormalMap;Normal Map;3;1;[Normal];Create;True;0;0;False;0;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-649.6832,-172.8447;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;16;-438.0961,209.5107;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-488.9544,-94.41182;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-197.248,-338.6721;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;12;-187.9608,468.4161;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-19.05083,-112.8395;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;HQ Modern FPS Weapons/Standard Weapon;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;17;0;4;4
WireConnection;11;0;17;0
WireConnection;11;1;10;0
WireConnection;25;0;22;1
WireConnection;25;1;22;2
WireConnection;25;2;22;3
WireConnection;15;0;4;2
WireConnection;24;0;23;1
WireConnection;24;1;23;2
WireConnection;24;2;23;3
WireConnection;18;0;15;0
WireConnection;18;1;9;0
WireConnection;27;0;24;0
WireConnection;27;1;25;0
WireConnection;16;0;4;1
WireConnection;26;0;23;4
WireConnection;26;1;22;4
WireConnection;20;0;19;0
WireConnection;20;1;21;0
WireConnection;12;0;11;0
WireConnection;0;0;27;0
WireConnection;0;1;3;0
WireConnection;0;2;20;0
WireConnection;0;3;16;0
WireConnection;0;4;12;0
WireConnection;0;5;18;0
WireConnection;0;10;26;0
ASEEND*/
//CHKSM=BE85DF370230E602AEC0EFCF1B790F78E421A195