Shader "Utility/Projective Transform by Corners" {
	Properties {
		_MainTex("Texture", 2D) = "white" {}

		_UvBottomLeft("UV Bottom Left", Vector) = (0, 0, 0, 0)
		_UvBottomRight("UV Bottom Right", Vector) = (1, 0, 0, 0)
		_UvTopLeft("UV Top Left", Vector) = (0, 1, 0, 0)
		_UvTopRight("UV Top Right", Vector) = (1, 1, 0, 0)
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram

			#include "UnityCG.cginc"

			struct VertexInput {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct FragmentInput {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			FragmentInput VertexProgram(VertexInput v) {
				FragmentInput o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;

			float4 _UvBottomLeft;
			float4 _UvBottomRight;
			float4 _UvTopLeft;
			float4 _UvTopRight;

			fixed4 FragmentProgram(FragmentInput i) : SV_Target {
				// Restore projected coordinates back to 3D space.
				_UvBottomLeft.xy = _UvBottomLeft.xy * _UvBottomLeft.z;
				_UvBottomRight.xy = _UvBottomRight.xy * _UvBottomRight.z;
				_UvTopLeft.xy = _UvTopLeft.xy * _UvTopLeft.z;
				_UvTopRight.xy = _UvTopRight.xy * _UvTopRight.z;

				float2 uv = i.uv;

				float3 bottom = lerp(_UvBottomLeft, _UvBottomRight, uv.x);
				float3 top = lerp(_UvTopLeft, _UvTopRight, uv.x);

				float3 samplePoint = lerp(bottom, top, uv.y);

				// Project back to screen.
				samplePoint.xy = samplePoint.xy / samplePoint.z;

				return tex2D(_MainTex, samplePoint);
			}
			ENDCG
		}
	}
}
