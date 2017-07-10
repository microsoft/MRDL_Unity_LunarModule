// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Dithering Shaders/Cutout/Unlit (Additive)" {
	Properties {
		_Color("Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_ColorCount ("Mixed Color Count", float) = 4
		_PaletteHeight ("Palette Height", float) = 128
		_PaletteTex ("Palette", 2D) = "black" {}
		_DitherSize ("Dither Size (Width/Height)", float) = 8
		_DitherTex ("Dither", 2D) = "black" {}
	}

	SubShader {
		Tags { "IgnoreProjector"="True" "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
		LOD 110

		AlphaTest Greater 0.5 
		Blend One One
		Cull Back
		ZWrite Off
		Lighting Off

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "CGIncludes/Dithering.cginc"

			sampler2D _MainTex;
			sampler2D _PaletteTex;
			sampler2D _DitherTex;
			float _ColorCount;
			float _PaletteHeight;
			float _DitherSize;
			float _Cutoff;
			fixed4 _Color;

			struct VertexInput {
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct FragmentInput {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 ditherPos : TEXCOORD1;
			};

			FragmentInput vert(VertexInput i) {
				FragmentInput o;
				o.position = UnityObjectToClipPos(i.position);
				o.uv = i.uv;
				o.ditherPos = GetDitherPos(i.position, _DitherSize);
				return o;
			}

			fixed4 frag(FragmentInput i) : COLOR {
				fixed4 c = tex2D(_MainTex, i.uv) * _Color;
				return fixed4(GetDitherColor(c.rgb, _DitherTex, _PaletteTex,
					_PaletteHeight, i.ditherPos, _ColorCount), c.a);
			}
			ENDCG
		}
	}

	Fallback "Unlit/Transparent Cutout"
}