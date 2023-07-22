// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Example/Sample2DArrayTexture"
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
					float4 vertex : POSITION;
					float2 lightData : TEXCOORD1;
					float3 voxelPos : TEXCOORD2;
				};

				v2f vert(float4 vertex : POSITION, float3 uv : TEXCOORD0, float2 lightInfo : TEXCOORD1, float3 voxelPos : TEXCOORD2)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(vertex);

					/*
					o.uv.xy = (uv.xy + 0.5) * _UVScale;
					o.uv.z = (uv.z + 0.5) * _SliceRange;
					*/
					//o.uv.xy = (uv.xy + 0.5) * _UVScale;
					//o.uv.z = (uv.z + 0.5) * _SliceRange;
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
					/*
					i.uv.x = fmod(((i.uv.x + 0.0001) * _TileSize),_TileSize);
					i.uv.y = fmod(((i.uv.y + 0.0001) * _TileSize), _TileSize);
					*/
					/*
					i.uv.x = fmod(i.uv.x, 1.0) * _TileSize;
					i.uv.y = fmod(i.uv.y, 1.0) * _TileSize;
					*/
					//i.uv.xy = frac(i.uv.xy);
					/*
					i.uv.xy = i.uv.xy + float2(_TileSizeX, _TileSizeY);
					i.uv.x = frac(i.uv.x * _TileSizeX);
					i.uv.y = frac(i.uv.y * _TileSizeY);
					*/
					//i.uv.x = frac(i.uv.x / _TileSizeX);
					//i.uv.y = frac(i.uv.y / _TileSizeY);
			
			
					//i.uv.xy = i.uv.xy / i.tileData;
					// debug from editor
					/*
					i.uv.x = i.uv.x / _TileSizeX;
					i.uv.y = i.uv.y / _TileSizeY;
					*/
					//return UNITY_SAMPLE_TEX2DARRAY(_MyArr, i.uv);
					half4 color = UNITY_SAMPLE_TEX2DARRAY(_MyArr, i.uv);
					float ca = tex2D(_DamageTex, i.uv.xy).a;
					// vertices for voxel @ (110, 99, 60) are:
					// 109.5, 99.5, 60.5
					// 110.5, 99.5, 60.5
					// 110.5, 99.5, 59.5
					// 109.5, 99.5, 59.5
					//
					// 109.5, 98.5, 60.5
					// 110.5, 98.5, 60.5
					// 110.5, 98.5, 59.5
					// 109.5, 98.5, 59.5
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