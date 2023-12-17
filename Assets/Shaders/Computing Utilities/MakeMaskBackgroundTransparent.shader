Shader "NaniCore/MakeMaskBackgroundTransparent" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
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
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				vs.vertex = UnityObjectToClipPos(vertex);
				vs.uv = uv;
				return vs;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				float4 color = tex2D(_MainTex, vs.uv);
				if(length(color.rgb) < .5f * sqrt(3))
					color = float4(0, 0, 0, 0);
				ps.target00 = color;
				return ps;
			}
			ENDCG
		}
	}
}