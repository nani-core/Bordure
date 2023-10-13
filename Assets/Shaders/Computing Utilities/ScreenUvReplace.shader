Shader "NaniCore/ScreenUvReplace" {
	SubShader {
		Pass {
			Cull Off
			ZTest Always
			ZWrite Off

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertex_shader
			#pragma fragment pixel_shader
			
			struct structureVS {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
 
			struct structurePS {
				float4 target00 : SV_Target0;
			};

			sampler2D _OriginalTex;
			sampler2D _ReplaceScreenTex;
			sampler2D _ScreenUvTex;
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				vs.vertex = UnityObjectToClipPos(vertex);
				vs.uv = uv;
				return vs;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				float2 screenUv = tex2D(_ScreenUvTex, vs.uv);
				if(false) {
					ps.target00 = float4(screenUv, 0, 1);
					return ps;
				}
				float4 whatScreenSample = tex2D(_ReplaceScreenTex, screenUv);
				if(length(whatScreenSample) >= 1.f / 256) {
					// If is in stamp area; that is, sampled what UV != (0, 0).
					ps.target00 = whatScreenSample;
					return ps;
				}
				ps.target00 = tex2D(_OriginalTex, vs.uv);
				return ps;
			}
			ENDCG
		}
	}
}