Shader "Dithering Shaders/Normal/Diffuse Simple" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ColorCount ("Mixed Color Count", float) = 4
		_PaletteHeight ("Palette Height", float) = 128
		_PaletteTex ("Palette", 2D) = "black" {}
	}

	SubShader {
		Tags { "IgnoreProjector"="True" "RenderType"="Opaque" }
		LOD 200

		BlendOp Max

		CGPROGRAM
		#pragma surface surf Lambert finalcolor:dither
		#include "CGIncludes/Dithering.cginc"

		sampler2D _MainTex;
		sampler2D _PaletteTex;
		sampler2D _DitherTex;
		float _ColorCount;
		float _PaletteHeight;
		float _DitherSize;

		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
		};

		void surf(Input i, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, i.uv_MainTex);
			o.Albedo = c.rgb;
		}

		void dither(Input i, SurfaceOutput o, inout fixed4 color) {
			color.rgb = GetDitherColorSimple(color.rgb, _PaletteTex,
				_PaletteHeight, i.screenPos, _ColorCount);
		}
		ENDCG
	}

	Fallback "Diffuse"
}