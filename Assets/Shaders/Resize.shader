Shader "NaniCore/Crop" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
		_Size ("Size", Vector) = (1920, 1080, 0, 1)
		_Range ("Range", Vector) = (1920, 1080, 0, 0)
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
			float4 _Size;
			float4 _Range;
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				vs.vertex = UnityObjectToClipPos(vertex);
				vs.uv = uv;
				return vs;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				float2 uv = (vs.uv * _Range.xy + _Range.zw) / _Size.xy;
				ps.target00 = tex2D(_MainTex, uv);
				return ps;
			}
			ENDCG
		}
	}
}