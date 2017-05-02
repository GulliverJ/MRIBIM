
Shader "Custom/ComponentOutline"
{
	Properties
	{
		_LineColor("Line Color", Color) = (1,1,1,1)
		_GridColor("Grid Color", Color) = (0,0,0,1)
		_LineWidth("Line Width", float) = 0.2
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
		
		a = (abs(i.texcoord1.y - 0.5) <= 0.2);//1 - abs((i.texcoord1.y - 0.5) * 1.4);//(0.5 + (i.texcoord1.y / 2));
	}
	else if (ly + hy == 1) {
		a = (abs(i.texcoord1.x - 0.5) <= 0.2);
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
	}*/
	

	answer = lerp(_LineColor, _GridColor, a/*min(lx*ly*hx*hy,a)*/);

	//answer.a = a;

	return answer;
	}
		ENDCG
	}
	}

		Fallback "Vertex Colored", 1
}