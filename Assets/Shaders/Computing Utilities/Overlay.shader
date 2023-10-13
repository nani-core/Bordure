Shader "NaniCore/Overlay" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
		_OverlayTex ("Overlay Texture", 2D) = "black" {}
		_Opacity ("Opacity", float) = 1
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
			sampler2D _OverlayTex;
			float _Opacity;
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				vs.vertex = UnityObjectToClipPos(vertex);
				vs.uv = uv;
				return vs;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				float2 uv = vs.uv;
				float4 a = tex2D(_MainTex, uv), b = tex2D(_OverlayTex, uv);
				ps.target00 = lerp(a, b, _Opacity * b.a);
				return ps;
			}
			ENDCG
		}
	}
}