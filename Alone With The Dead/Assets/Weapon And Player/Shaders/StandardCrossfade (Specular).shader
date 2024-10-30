Shader "HQ FPS Weapons/Standard Crossfade (Specular)"
{
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Cutoff ("Cutoff", Range(0,1)) = 0.5
		_MainTex ("Albedo", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_Specular ("Specular", 2D) = "black" {}
		_Occlusion("Occlusion", 2D) = "white" {}
		[HideInInspector] _FlatMap ("Flat Normal Map", 2D) = "bump" {}
		_BumpIntensity ("Normal Intensity", Range(0,1)) = 1
		_Glossiness ("Smoothness", Range(0.0, 1.0)) = 0.5
	}
	SubShader 
	{
		Tags
		{ 	"RenderType"="Opaque"
             "IgnoreProjector"="True"
             "RenderType"="Grass"
			// "DisableBatching"="True"
		}
		LOD 300
		//Cull off
		
		
		CGPROGRAM
		#pragma multi_compile _ LOD_FADE_CROSSFADE
		#pragma surface surf StandardSpecular alphatest:_Cutoff
		#pragma target 3.0
		
		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _FlatMap;
		sampler2D _Specular;
		sampler2D _Occlusion;

		struct Input 
		{
			float4 screenPos;
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_Specular;
			float2 uv_Occlusion;
		};
		
		half _BumpIntensity;
		half _Glossiness;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) 
		{
		 	#ifdef LOD_FADE_CROSSFADE
            float2 vpos = IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy;
            UnityApplyDitherCrossFade(vpos);
            #endif

			fixed4 tex = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = tex.rgb;
			fixed3 normal1 = UnpackNormal(tex2D(_FlatMap, IN.uv_BumpMap));
			fixed3 normal2 = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			o.Normal = lerp(normal1, normal2, _BumpIntensity);
			o.Smoothness = tex2D(_Specular, IN.uv_Specular).a * _Glossiness;
			o.Specular = tex2D(_Specular, IN.uv_MainTex).rgb;
			o.Alpha = tex.a;

			o.Occlusion = tex2D(_Occlusion, IN.uv_Occlusion).g;
		}
		 
		ENDCG
	}
	FallBack "Diffuse"
}
