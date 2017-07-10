Shader "HUX/Tri-Planar Pulse Surface" {
	Properties{
		_Color("Color", Color) = (0,0,0,0)
		_PulseColor ("Pulse Color", Color) = (0,0,0,0)
		_SideX("SideX", 2D) = "white" {}
		_SideZ("SideZ", 2D) = "white" {}
		_Top("Top", 2D) = "white" {}
		_SideScale("Side Scale", Float) = 2
		_TopScale("Top Scale", Float) = 2
		_PulseCenter("Pulse Center", Vector) = (0,0,0)
		_PulseSize("Pulse Size", Range(0,10)) = 3.5
		_PulseWidthFront("Pulse Width (Front)", Range(0.01, 1)) = 0.5
		_PulseWidthBack("Pulse Width (Back)", Range(0.01, 1)) = 1
		_MinVisibility("Min Visibility", Range(0,1)) = 0
	}
		SubShader{
			Pass{ ColorMask 0 }

			Tags{
				//"Queue" = "Transparent"
				//"IgnoreProjector" = "True"
				//"RenderType" = "Transparent"
			}

			Cull Back
			ZWrite Off
			//Blend One One

			CGPROGRAM
			#pragma surface surf Lambert
			#pragma exclude_renderers flash

			sampler2D _SideX;
			sampler2D _SideZ;
			sampler2D _Top;
			fixed _SideScale;
			fixed _TopScale;
			fixed3 _PulseCenter;
			fixed _PulseSize;
			fixed _PulseWidthFront;
			fixed _PulseWidthBack;
			fixed _MinVisibility;
			fixed4 _Color;
			fixed4 _PulseColor;

			struct Input {
				float3 worldPos;
				float3 worldNormal;
			};

			void surf(Input IN, inout SurfaceOutput o) {
				float dist = distance(IN.worldPos, _PulseCenter.xyz);
				float pulse = 0;
				if (dist > _PulseSize) {
					pulse = (1 - clamp(dist - _PulseSize, 0, 1) / _PulseWidthFront);
				}
				else {
					pulse = (1 - clamp(_PulseSize - dist, 0, 1) / _PulseWidthBack);
				}
				
				float3 projNormal = saturate(pow(IN.worldNormal * 1.4, 4));
				float3 side = tex2D(_SideX, frac(IN.worldPos.zy * _SideScale)) * abs(IN.worldNormal.x);
				float3 front = tex2D(_SideZ, frac(IN.worldPos.xy * _SideScale)) * abs(IN.worldNormal.z);
				float3 top = tex2D(_Top, frac(IN.worldPos.zx * _TopScale)) * abs(IN.worldNormal.y);

				/*if (IN.worldNormal.y > 0) {
					top = tex2D(_Top, frac(IN.worldPos.zx * _TopScale)) * IN.worldNormal.y;
				}
				else {
					bottomAlphaSub = abs(IN.worldNormal.y);
				}*/

				o.Albedo = 0;
				o.Emission = front;
				o.Emission = lerp(o.Emission, side, projNormal.x);
				o.Emission = lerp(o.Emission, top, projNormal.y);
				o.Emission *= (_Color * max(pulse, _MinVisibility));
				o.Emission += _PulseColor * pulse * _Color.a;
				o.Alpha = 1;
			}
		ENDCG
		}
			Fallback "Diffuse"
}