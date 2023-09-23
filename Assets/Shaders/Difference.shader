Shader "NaniCore/Difference" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
		_DifferenceTex ("Difference Texture", 2D) = "black" {}
	}
	SubShader {
		Pass {
			Cull Off
			ZTest Always
			ZWrite Off

			CGPROGRAM
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
			sampler2D _DifferenceTex;
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				vs.vertex = UnityObjectToClipPos(vertex);
				vs.uv = uv;
				return vs;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				float2 uv = vs.uv;
				ps.target00 = float4(abs(tex2D(_MainTex, uv).xyz - tex2D(_DifferenceTex, uv).xyz), 1);
				return ps;
			}
			ENDCG
		}
	}
}