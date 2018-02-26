// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unity Point Cloud Loader
// (C) 2016 Ryan Theriot, Eric Wu, Jack Lam. Laboratory for Advanced Visualization & Applications, University of Hawaii at Manoa.
// Version: February 17th, 2017

Shader "PointCloudShader"
{
	Properties
	{
		_Size("Size", Range(0, 0.02)) = 0.0035
		[Enum(Circle,0, Square, 1)] _Shape("Point Shape", Float) = 1
		[Enum(False,0, True, 1)] _UniformColor("Apply Uniform Color", Float) = 0
		_Color("Color", Color) = (1, 1, 1, 1)
	}

		SubShader
	{
		Pass
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
#pragma target 5.0
#pragma vertex VS_Main
#pragma fragment FS_Main
#pragma geometry GS_Main
#include "UnityCG.cginc" 

		struct VS_INPUT
	{
		float4 vertex : POSITION;
		float4 color  : COLOR0;
	};
	struct GS_INPUT
	{
		float4 pos : SV_POSITION;
		float4 color : COLOR0;
	};

	struct FS_INPUT
	{
		float4	pos		: POSITION;
		float4 color : COLOR0;
		float2 uv : TEXCOORD0;
	};

	float _Size;
	float _Shape;
	float _UniformColor;
	float4 _Color;

	//Shader
	GS_INPUT VS_Main(VS_INPUT v)
	{
		GS_INPUT output = (GS_INPUT)0;

		output.color = v.color;
		output.pos = mul(unity_ObjectToWorld, v.vertex);

		return output;
	}

	//Geometry Shader
	[maxvertexcount(4)]
	void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
	{
		float3 up = normalize(cross(p[0].pos, _WorldSpaceCameraPos));
		float3 look = _WorldSpaceCameraPos - p[0].pos;
		look = normalize(look);
		float3 right = cross(up, look);

		float halfS = 0.5f * _Size;

		float4 v[4];
		v[0] = float4(p[0].pos + halfS * right - halfS * up, 1.0f);
		v[1] = float4(p[0].pos + halfS * right + halfS * up, 1.0f);
		v[2] = float4(p[0].pos - halfS * right - halfS * up, 1.0f);
		v[3] = float4(p[0].pos - halfS * right + halfS * up, 1.0f);

		//float4 vp = UnityObjectToClipPos(mul(unity_WorldToObject, v[0]));
		FS_INPUT pIn;
		pIn.color = p[0].color;
		pIn.pos = UnityObjectToClipPos(mul(unity_WorldToObject, v[0]));
		pIn.uv = float2(1.0f, 0.0f);
		triStream.Append(pIn);

		pIn.pos = UnityObjectToClipPos(mul(unity_WorldToObject, v[1]));
		pIn.uv = float2(1.0f, 1.0f);
		triStream.Append(pIn);

		pIn.pos = UnityObjectToClipPos(mul(unity_WorldToObject, v[2]));
		pIn.uv = float2(0.0f, 0.0f);
		triStream.Append(pIn);

		pIn.pos = UnityObjectToClipPos(mul(unity_WorldToObject, v[3]));
		pIn.uv = float2(0.0f, 1.0f);
		triStream.Append(pIn);
	}



	//Fragment Shader
	float4 FS_Main(FS_INPUT input) : COLOR
	{
		//Circle
		if (_Shape == 0)
		{
			float uvDistance = sqrt(pow(input.uv.x - 0.5, 2) + pow(input.uv.y - 0.5, 2));

			if (uvDistance < .5)
			{
				if (_UniformColor == 0)
					return input.color;
				else
					return _Color;
			}
			else
			{
				discard;
			}

			return input.color;
		}
	//Square
		else
		{
			if (_UniformColor == 0)
				return input.color;
			else
				return _Color;
		}

	}

		ENDCG
	}
	}
}