Shader "NaniCore/InfectByValue" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
		_Size ("Size", Vector) = (1920, 1080, 0, 1)
		_Value ("Value", Color) = (0, 0, 0, 1)
		_Radius ("Radius", Vector) = (1, 1, 0, 1)
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
			float4 _Value;
			float4 _Radius;
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				vs.vertex = UnityObjectToClipPos(vertex);
				vs.uv = uv;
				return vs;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				float4 color = tex2D(_MainTex, vs.uv);
				int2 radius = ceil(abs(_Radius.xy));
				for(int dx = -radius.x; dx <= radius.x; ++dx) {
					for(int dy = -radius.y; dy <= radius.y; ++dy) {
						float2 uv = vs.uv + float2(dx, dy) / _Size.xy;
						float4 thatColor = tex2D(_MainTex, uv);
						if(distance(thatColor.xyz, _Value.xyz) < 1.f / 256)
							color = _Value;
					}
				}
				ps.target00 = color;
				return ps;
			}
			ENDCG
		}
	}
}