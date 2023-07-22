// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TexOpaqueCutout" {

	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_CutoutTex("Cutout Albedo (RGB)", 2D) = "white" {}
		_Health("Health", Range(0,1)) = 0
	}
		Category {
		Tags { "RenderType" = "Opaque" }
		Lighting Off

		SubShader {
			Pass {

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					fixed4 color : COLOR;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				sampler2D _MainTex;
				sampler2D _CutoutTex;
				float _Health;

				v2f vert(appdata_t v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 color = tex2D(_MainTex, i.uv);
					float ca = tex2D(_CutoutTex, i.uv).a;

					if (ca > _Health)
					{
						color.rgb = 0;
						color.a = 1;
					}

					return color;
				}
				ENDCG
			}
		}
		}
}
