Shader "NaniCore/UvMap" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
	}
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

			sampler2D _MainTex;
			sampler2D _UvTex;
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				vs.vertex = UnityObjectToClipPos(vertex);
				vs.uv = uv;
				return vs;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				ps.target00 = tex2D(_MainTex, tex2D(_UvTex, vs.uv));
				return ps;
			}
			ENDCG
		}
	}
}