Shader "Hidden/Focus Animated Shader" {
	Properties {
		_FrontColor("Front Color", Color) = (1, 1, 1, 1)
		_BackColor ("Back Color" , Color) = (0.8, 0.8, 0.8, 1)
		_Radius ("Radius", Range(-.5, .5)) = .1
		_Width ("Width", Range(0, 1)) = .1
		_Opacity ("Opacity", Range(0, 1)) = 1
		_Smoothness ("Smoothness", Range(0, 1)) = .05
	}
	SubShader {
		Tags {
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
		}

		Cull Off
		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			float4 _FrontColor;
			float4 _BackColor;
			float _Radius;
			float _Width;
			float _Opacity;
			float _Smoothness;

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			float4 frag(v2f i) : SV_Target {
				float radiOffset = length(i.uv - .5f) - _Radius;
				float tolerance = _Width * .5f;
				float t = smoothstep(-_Smoothness, _Smoothness, abs(radiOffset) - tolerance);
				float3 color = lerp(_FrontColor, _BackColor, t);
				float opacity = lerp(1, 0, t) * _Opacity;
				return float4(color, opacity);
			}
			ENDCG
		}
	}
}
