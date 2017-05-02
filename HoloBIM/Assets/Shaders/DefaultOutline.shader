
Shader "Custom/DefaultOutline"
{
	Properties
	{
		_LineColor("Line Color", Color) = (1,1,1,1)
		_GridColor("Grid Color", Color) = (0,0,0,0)
		_LineWidth("Line Width", float) = 0.20
		_Feather("Feather", float) = 0.05
	}
		SubShader
	{
		Pass
	{
		Tags{ "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater 0.5

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

	uniform float4 _LineColor;
	uniform float4 _GridColor;
	uniform float _LineWidth;
	uniform float _Feather;

	// vertex input: position, uv1, uv2
	struct appdata
	{
		float4 vertex : POSITION;
		float4 texcoord1 : TEXCOORD0;
		float4 color : COLOR;
	};

	struct v2f
	{
		float4 pos : POSITION;
		float4 texcoord1 : TEXCOORD0;
		float4 color : COLOR;
	};

	v2f vert(appdata v)
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.texcoord1 = v.texcoord1;
		o.color = v.color;
		return o;
	}

	fixed4 frag(v2f i) : COLOR
	{
		fixed4 answer;

		// 1 if texcoord x is outside of line
		// So 1 if definitely outside
		float lx = step(_LineWidth, i.texcoord1.x);
		float ly = step(_LineWidth, i.texcoord1.y);

		float hx = step(i.texcoord1.x, 1.0 - _LineWidth);
		float hy = step(i.texcoord1.y, 1.0 - _LineWidth);

		float tx = 1;
		float ty = 1;
		float a = 1;

		//if ((lx * ly * hx * hy) == 0) {


		// If we're in the void...
		if (ly*hy*lx*ly == 1) {
			/*
			if (i.texcoord1.y < _LineWidth + _Feather) {
				ty = ((i.texcoord1.y - _LineWidth) / _Feather) *(abs((i.texcoord1.x - 0.5) * 1.4));
			}
			else if (i.texcoord1.y > 1 - (_LineWidth + _Feather)) {
				ty = (((1 - i.texcoord1.y) - _LineWidth) / _Feather) *(abs((i.texcoord1.x - 0.5) * 1.4));
			}

			if (i.texcoord1.x < _LineWidth + _Feather) {
				tx = ((i.texcoord1.x - _LineWidth) / _Feather) *(abs((i.texcoord1.y - 0.5) * 1.4));
			}
			else if (i.texcoord1.x > 1 - (_LineWidth + _Feather)) {
				tx = (((1 - i.texcoord1.x) - _LineWidth) / _Feather) *(abs((i.texcoord1.y - 0.5) * 1.4));
			}*/


			a = min(tx, ty);
			/*
			if (min(tx, ty) != 1) {
				if (tx < ty) {
					tx -= abs((i.texcoord1.y - 0.5) * 1.4);
					a = tx;
				}
				else {
					ty -= abs((i.texcoord1.x - 0.5) * 1.4);
					a = ty;
				}
			}*/

			//a = 1 - min(ty, tx);
		}

		
		if (lx + hx == 1) {
			if (i.texcoord1.y < 0.3) {
				a = 1 - abs((i.texcoord1.y - 0.3) * 1.4);//(0.5 + (i.texcoord1.y / 2));
			}
			else if (i.texcoord1.y > 0.7) {
				a = 1 - abs((i.texcoord1.y - 0.7) * 1.4);//(0.5 + (i.texcoord1.y / 2));
			}
			else {
				a = 1;
			}
			//a = 1 - min(abs((i.texcoord1.y - 0.3) * 1.4), abs((i.texcoord1.y - 0.7) * 1.4));//(0.5 + (i.texcoord1.y / 2));
		}
		else if (ly + hy == 1) {
			if (i.texcoord1.x < 0.3) {
				a = 1 - abs((i.texcoord1.x - 0.3) * 1.4);//(0.5 + (i.texcoord1.y / 2));
			}
			else if (i.texcoord1.x > 0.7) {
				a = 1 - abs((i.texcoord1.x - 0.7) * 1.4);//(0.5 + (i.texcoord1.y / 2));
			}
			else {
				a = 1;
			}
			//a = 1 - min(abs((i.texcoord1.x - 0.3) * 1.4), abs((i.texcoord1.x - 0.7) * 1.4));
		}


		


		/*	Useful for doing hte corner bits only	
		if ((lx * ly * hx * hy) == 0) {
			if (i.texcoord1.x < 0.5) {
				tx = max(((i.texcoord1.x - _LineWidth)) / _LineWidth, 0);
			}
			else {
				tx = max((((1 - i.texcoord1.x) - _LineWidth)) / _LineWidth, 0);
			}

			if (i.texcoord1.y < 0.5) {
				ty = max(((i.texcoord1.y - _LineWidth)) / _LineWidth, 0);
			}
			else {
				ty = max((((1 - i.texcoord1.y) - _LineWidth)) / _LineWidth, 0);
			}

			a = max(ty, tx);
		}
		*/

		
		answer = lerp(_LineColor, _GridColor, a/*min(lx*ly*hx*hy,a)*/);
			
		//answer.a = a;

		return answer;
		}
			ENDCG
		}
	}

	Fallback "Vertex Colored", 1
}

/*
Shader "Custom/DefaultOutline"
{
	Properties
	{
		_BaseColor("Base color", Color) = (1.0, 1.0, 1.0, 0.0)
		_WireColor("Wire color", Color) = (1.0, 1.0, 1.0, 1.0)
		_WireThickness("Wire thickness", Range(0, 800)) = 100
	}
		SubShader
	{
		Tags{ "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater 0.5

		Pass{
		Offset 50, 100

		CGPROGRAM

#pragma vertex vert
#pragma geometry geom
#pragma fragment frag

		//we only target the hololens (and the unity editor) so take advantage of shader model 5
#pragma target 5.0
#pragma only_renderers d3d11

#include "UnityCG.cginc"

	float4 _BaseColor;
	float4 _WireColor;
	float _WireThickness;

	// Based on approach described in "Shader-Based Wireframe Drawing", http://cgg-journal.com/2008-2/06/index.html

	struct v2g
	{
		float4 viewPos : POSITION;
	};

	v2g vert(appdata_base v)
	{
		v2g o;
		o.viewPos = mul(UNITY_MATRIX_MVP, v.vertex);
		return o;
	}

	// inverseW is to counter-act the effect of perspective-correct interpolation so that the lines look the same thickness
	// regardless of their depth in the scene.
	struct g2f
	{
		float4 viewPos : POSITION;
		float inverseW : TEXCOORD0;
		float3 dist : TEXCOORD1;
		float3 vert : TEXCOORD2;
	};

	[maxvertexcount(3)]
	void geom(triangle v2g i[3], inout TriangleStream<g2f> triStream)
	{
		// Calculate the vectors that define the triangle from the input points.
		float2 point0 = i[0].viewPos.xy / i[0].viewPos.w;
		float2 point1 = i[1].viewPos.xy / i[1].viewPos.w;
		float2 point2 = i[2].viewPos.xy / i[2].viewPos.w;

		// Calculate the area of the triangle.
		float2 vector0 = point2 - point1;
		float2 vector1 = point2 - point0;
		float2 vector2 = point1 - point0;
		float area = abs(vector1.x * vector2.y - vector1.y * vector2.x) / 2;

		float wireScale = 800 - _WireThickness;

		// Output each original vertex with its distance to the opposing line defined
		// by the other two vertices.

		g2f o;

		// Need to exclude verticals
		// 

		o.viewPos = i[0].viewPos;
		o.inverseW = 1.0 / o.viewPos.w;
		//o.dist = float3(area / length(vector0), 0, 0) * o.viewPos.w * wireScale;
		if (dot(vector0, vector1) == 1 || dot(vector0, vector2) == 1) {
			o.vert = float3(1, 0, 0);
			o.dist = float3(area / length(vector0), 0, 0) * o.viewPos.w * wireScale * 10000;
		}
		else {
			o.dist = float3(area / length(vector0), 0, 0) * o.viewPos.w * wireScale;
			o.vert = float3(0, 0, 0);
		}
		triStream.Append(o);

		o.viewPos = i[1].viewPos;
		o.inverseW = 1.0 / o.viewPos.w;
		//o.dist = float3(0, area / length(vector1), 0) * o.viewPos.w * wireScale;
		if (dot(vector1, vector0) == 1 || dot(vector1, vector2) == 1) {
			o.vert = float3(0, 1, 0);
			o.dist = float3(0, area / length(vector1), 0) * o.viewPos.w * wireScale * 10000;
		}
		else {
			o.vert = float3(0, 0, 0);
			o.dist = float3(0, area / length(vector1), 0) * o.viewPos.w * wireScale;
		}
		triStream.Append(o);

		o.viewPos = i[2].viewPos;
		o.inverseW = 1.0 / o.viewPos.w;
		//o.dist = float3(0, 0, area / length(vector2)) * o.viewPos.w * wireScale;
		if (dot(vector2, vector0) == 1 || dot(vector2, vector1) == 1) {
			o.vert = float3(0, 0, 1);
			o.dist = float3(0, 0, area / length(vector2)) * o.viewPos.w * wireScale * 10000;
		}
		else {
			o.vert = float3(0, 0, 0);
			o.dist = float3(0, 0, area / length(vector2)) * o.viewPos.w * wireScale;
		}

		triStream.Append(o);
	}

	float4 frag(g2f i) : COLOR
	{
		// Calculate  minimum distance to one of the triangle lines, making sure to correct
		// for perspective-correct interpolation.

		float dist = 0;

		// TODO Have to check which edge is vertical here and only measure dist from those edges
		if (i.vert[0] == 1) {
			if (i.vert[1] == 1) {
				dist = min(i.dist[0], i.dist[1]);
			}
			else {
				dist = min(i.dist[0], i.dist[2]);
			}
		}
		else if (i.vert[1] == 1) {
			if (i.vert[2] == 1) {
				dist = min(i.dist[1], i.dist[2]);
			}
		}
		else {
			dist = min(i.dist[0], min(i.dist[1], i.dist[2]));// *i.inverseW;// - maintains width
		}
		dist = min(i.dist[0], min(i.dist[1], i.dist[2]));// *i.inverseW;// - maintains width
		


		// Make the intensity of the line very bright along the triangle edges but fall-off very
		// quickly.

		//float I = exp2(-2 * dist * dist);
		float I = exp2(-2 * dist * dist * dist);

		// Fade out the alpha but not the color so we don't get any weird halo effects from
		// a fade to a different color.
		float4 color = I * _WireColor;// +(1 - I) * _BaseColor;
	
		//float4 color = _WireColor;	

		color.a = I;
		return color;
		}

			ENDCG
		}
	}
	FallBack "Vertex Colored", 1
}
*/