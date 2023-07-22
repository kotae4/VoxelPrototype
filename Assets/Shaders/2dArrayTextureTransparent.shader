// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Example/Sample2DArrayTextureTransparent"
{
	Properties
	{
		_MyArr("Tex", 2DArray) = "" {}
		_DamageTex("DamageTex", 2D) = "white" {}
		_Health("Health", Range(0,1)) = 0
		_DamagePos("DamagePos", Vector) = (0,0,0,0)
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

	Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
		// to use texture arrays we need to target DX10/OpenGLES3 which
		// is shader model 3.5 minimum
#pragma target 3.5


#include "UnityCG.cginc"

		struct v2f
		{
			float3 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
			float2 lightData : TEXCOORD1;
			float3 voxelPos : TEXCOORD2;
		};

		v2f vert(float4 vertex : POSITION, float3 uv : TEXCOORD0, float2 lightInfo : TEXCOORD1, float3 voxelPos : TEXCOORD2)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(vertex);
			o.uv = uv;
			o.lightData = lightInfo;
			o.voxelPos = voxelPos;
			return o;
		}

		UNITY_DECLARE_TEX2DARRAY(_MyArr);
		sampler2D _DamageTex;
		float _Health;
		float4 _DamagePos;

		half4 frag(v2f i) : SV_Target
		{
			//return UNITY_SAMPLE_TEX2DARRAY(_MyArr, i.uv) * i.lightData.x;

			half4 color = UNITY_SAMPLE_TEX2DARRAY(_MyArr, i.uv);
			float ca = tex2D(_DamageTex, i.uv.xy).a;

			if (all(i.voxelPos.xyz == _DamagePos.xyz))
			{
				if (ca > _Health)
				{
					color.rgb = 0;
					color.a = 1;
				}
			}

			return color * i.lightData.x;
		}
		ENDCG
	}
	}
}