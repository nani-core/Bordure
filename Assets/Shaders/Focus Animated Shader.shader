Shader "Hidden/Focus Animated Shader" {
	Properties {
		_FrontColor("Front Color", Color) = (1, 1, 1, 1)
		_BackColor ("Back Color" , Color) = (0.8, 0.8, 0.8, 1)
		_Radius ("Radius", Range(-.5, .5)) = .1
		_Width ("Width", Range(0, 1)) = .1
		_Opacity ("Opacity", Range(0, 1)) = 1
		_Smoothness ("Smoothness", Range(0, 1)) = .05
		[KeywordEnum(Circle, Square)] _Shape ("Shape", Int) = 0
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
			#pragma shader_feature _SHAPE_CIRCLE _SHAPE_SQUARE

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

#if defined(_SHAPE_CIRCLE)
			#define set_offset set_circle
			inline float set_circle(v2f i, float a) {
				return length(i.uv - .5f) - a;
			}
#elif defined(_SHAPE_SQUARE)
			#define set_offset set_square
			inline float set_square(v2f i, float a) {
				return max(abs(i.uv.x - .5f), abs(i.uv.y - .5f)) - a;
			}
#endif

			float4 frag(v2f i) : SV_Target {
				float radiOffset = set_offset(i, _Radius);
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
