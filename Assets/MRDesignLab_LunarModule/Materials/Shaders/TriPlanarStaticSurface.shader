Shader "HUX/Tri-Planar Static Surface" {
	Properties{
		_Color("Color", Color) = (0,0,0,0)
		_SideX("SideX", 2D) = "white" {}
		_SideZ("SideZ", 2D) = "white" {}
		_Top("Top", 2D) = "white" {}
		_SideScale("Side Scale", Float) = 2
		_TopScale("Top Scale", Float) = 2
		_GradientCenterA("Gradient Center A", Vector) = (0,0,0)
		_GradientCenterB("Gradient Center B", Vector) = (0,0,0)
		_GradientMaxRange("Max Range", Range(0,10)) = 3.5
		_GradientMinRange("Min Range", Range(0,10)) = 0.25
		_MinAlpha ("Min Alpha", Range(0,1)) = 0
		_CeilingOpacity ("Ceiling Opacity", Range (0, 1)) = 0
		_CeilingHeight ("Ceiling Height", Float) = 1000
		_CeilingTransition ("Ceiling Transition", Range (0, 5)) = 0.5
	}
		SubShader{
			Pass{ ColorMask 0 }

			Cull Back
			ZWrite On
			ZTest LEqual

			CGPROGRAM
			#pragma surface surf Lambert alpha
			#pragma exclude_renderers flash

			sampler2D _SideX;
			sampler2D _SideZ;
			sampler2D _Top;
			fixed3 _GradientCenterA;
			fixed3 _GradientCenterB;
			fixed _GradientMaxRange;
			fixed _GradientMinRange;
			fixed _SideScale;
			fixed _TopScale;
			fixed _MinAlpha;
			fixed _CeilingOpacity;
			fixed _CeilingHeight;
			fixed _CeilingTransition;
			fixed4 _Color;

			struct Input {
				float3 worldPos;
				float3 worldNormal;
			};

			void surf(Input IN, inout SurfaceOutput o) {
				float3 projNormal = saturate(pow(IN.worldNormal * 1.4, 4));
				float3 side = tex2D(_SideX, frac(IN.worldPos.zy * _SideScale)) * abs(IN.worldNormal.x);
				float3 front = tex2D(_SideZ, frac(IN.worldPos.xy * _SideScale)) * abs(IN.worldNormal.z);
				float topAbsNormal = abs(IN.worldNormal.y);
				float3 top = tex2D(_Top, frac(IN.worldPos.zx * _TopScale)) * topAbsNormal;

				float ceilingSubtract = 0;
				// If it's a ceiling
				if (IN.worldNormal.y < 0) {
					ceilingSubtract = ((1 - _CeilingOpacity) * topAbsNormal);
				}

				float heightSubtract = 0;
				// If we're above ceiling height
				if (IN.worldPos.y > _CeilingHeight) {
					heightSubtract = clamp((IN.worldPos.y - _CeilingHeight) / _CeilingTransition, 0, 1);
				}

				float gradientA = clamp (1 - ((distance(IN.worldPos, _GradientCenterA.xyz) - _GradientMinRange) / _GradientMaxRange), 0, 1);
				float gradientB = clamp (1 - ((distance(IN.worldPos, _GradientCenterB.xyz) - _GradientMinRange) / _GradientMaxRange), 0, 1);

				o.Albedo = front;
				o.Albedo = lerp(o.Albedo, side, projNormal.x);
				o.Albedo = lerp(o.Albedo, top, projNormal.y);
				o.Albedo *= _Color;
				o.Alpha = clamp (clamp((_MinAlpha - ceilingSubtract) + (gradientA + gradientB), 0, 1) - heightSubtract, 0, 1) * _Color.a;
			}
		ENDCG
		}
			Fallback "Diffuse"
}