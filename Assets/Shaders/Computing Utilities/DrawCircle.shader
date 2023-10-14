Shader "NaniCore/DrawCircle" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
		_Size ("Size", Vector) = (0, 0, 0, 1)
		_Radius ("Radius", float) = 10
		_Offset ("Offset", Vector) = (0, 0, 0, 1)
		_Color ("Color", Color) = (1, 1, 1, 1)
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
			float4 _Offset;
			float _Radius;
			float4 _Color;
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				vs.vertex = UnityObjectToClipPos(vertex);
				vs.uv = uv;
				return vs;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				float2 pos = vs.uv * _Size.xy;
				float2 dPos = _Offset.xy - pos;
				bool inRange = length(dPos) <= _Radius;
				ps.target00 = inRange ? _Color : tex2D(_MainTex, vs.uv);
				return ps;
			}
			ENDCG
		}
	}
}