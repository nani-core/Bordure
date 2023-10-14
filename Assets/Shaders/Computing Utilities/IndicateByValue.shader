Shader "NaniCore/IndicateByValue" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
		_Value ("Value", Color) = (0, 0, 0, 1)
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
			float4 _Value;
			float _Tolerance;
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				vs.vertex = UnityObjectToClipPos(vertex);
				vs.uv = uv;
				return vs;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				float4 color = tex2D(_MainTex, vs.uv);
				bool yes = distance(color, _Value) < _Tolerance / 256;
				ps.target00 = float4(float3(1, 1, 1) * (yes ? 1 : 0), 1);
				return ps;
			}
			ENDCG
		}
	}
}