Shader "NaniCore/ReplaceValueByValue" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
		_Value ("Value", Color) = (0, 0, 0, 1)
		_ReplaceValue ("Replace Texture", Color) = (1, 1, 1, 1)
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
			float4 _ReplaceValue;
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				vs.vertex = UnityObjectToClipPos(vertex);
				vs.uv = uv;
				return vs;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				float4 value = tex2D(_MainTex, vs.uv);
				if(distance(value, _Value) < 1.f / 256)
					ps.target00 = _ReplaceValue;
				else
					ps.target00 = value;
				return ps;
			}
			ENDCG
		}
	}
}