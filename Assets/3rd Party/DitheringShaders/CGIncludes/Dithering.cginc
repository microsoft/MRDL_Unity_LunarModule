// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#ifndef DITHERING_INCLUDED
#define DITHERING_INCLUDED

#include "UnityCG.cginc"

inline float4 GetDitherPos(float4 vertex, float ditherSize) {
	// Get the dither pixel position from the screen coordinates.
	float4 screenPos = ComputeScreenPos(UnityObjectToClipPos(vertex));
	return float4(screenPos.xy * _ScreenParams.xy / ditherSize, 0, screenPos.w);
}

inline fixed3 GetDitherColor(fixed3 color, sampler2D ditherTex, sampler2D paletteTex,
							 float paletteHeight, float4 ditherPos, float colorCount) {
	// To find the palette color to use for this pixel:
	//	The row offset decides which row of color squares to use.
	//	The red component decides which column of color squares to use.
	//	The green and blue components points to the color in the 16x16 pixel square.
	float ditherValue = tex2D(ditherTex, ditherPos.xy / ditherPos.w).r;
	float2 paletteUV = float2(
		min(floor(color.r * 16), 15) / 16 + clamp(color.b * 16, 0.5, 15.5) / 256,
		(clamp(color.g * 16, 0.5, 15.5) + floor(ditherValue * colorCount) * 16) / paletteHeight);

	// Return the new color from the palette texture
	return tex2D(paletteTex, paletteUV).rgb;
}

inline fixed3 GetDitherColorSimple(fixed3 color, sampler2D paletteTex, float paletteHeight,
								   float4 screenPos, float colorCount) {
	// A simplified version of the GetDitherColor function which uses
	// a fixed 4 color matrix and a 1:1 pixel size.
	screenPos.xy = floor((screenPos.xy / screenPos.w) *_ScreenParams.xy) + 0.01;
	float rowOffset = floor((fmod(screenPos.x, 2) * 0.249 +
		fmod(screenPos.x + screenPos.y, 2) * 0.499) * colorCount) * 16;

	float2 paletteUV = float2(
		clamp(floor(color.r * 16), 0, 15) / 16 + clamp(color.b * 16, 0.5, 15.5) / 256,
		(clamp(color.g * 16, 0.5, 15.5) + rowOffset) / paletteHeight);

	return tex2D(paletteTex, paletteUV).rgb;
}

#endif