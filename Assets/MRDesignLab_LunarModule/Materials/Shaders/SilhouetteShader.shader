Shader "HUX/Silhouette Fresnel"
{
	Properties
	{
		_Color("Main Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_RimColor("Rim Color", Color) = (0.26,0.19,0.16,0.0)
		_RimPower("Rim Power", Range(0.5,8.0)) = 3.0
	}
	
	SubShader
	{
		Pass{ ColorMask 0 }

		Tags{
			"Queue" = "Geometry"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Cull Back
		ZWrite Off
		ZTest Never
		Blend One One

		CGPROGRAM
		#pragma surface surf NoLighting alpha

		struct Input
		{
			float3 viewDir;
		};

		fixed4 _Color;
		fixed4 _RimColor;
		fixed _RimPower;

		half4 LightingNoLighting(SurfaceOutput s, half3 lightDir, half atten) {
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			o.Alpha = _Color.a;
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
			o.Albedo = _Color.rgb + _RimColor.rgba * pow(rim, _RimPower);
		}
		ENDCG
	}
	Fallback "Diffuse"
}