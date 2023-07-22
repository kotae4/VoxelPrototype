// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/VColorOpaque" {

	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}
	Category {
	Tags { "RenderType"="Opaque" }
	Lighting Off

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			fixed4 _Color;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = _Color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG 
		}
	}	
}
}
