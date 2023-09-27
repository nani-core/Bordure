Shader "NaniCore/ScreenUvReplace" {
	Properties {
		_OriginalTex ("Original Texture", 2D) = "white" {}
		_ReplaceScreenTex ("Replace Texture", 2D) = "black" {}
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
				float3 worldPos : TEXCOORD1;
			};
 
			struct structurePS {
				float4 target00 : SV_Target0;
			};

			sampler2D _OriginalTex;
			sampler2D _ReplaceScreenTex;
			float4x4 _Projection;
			float4x4 _WhereToWorld;
			float _ScreenRatio;
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				vs.vertex = UnityObjectToClipPos(vertex);
				vs.uv = uv;
				vs.worldPos = mul(_WhereToWorld, vs.vertex);
				return vs;
			}

			float2 WorldToScreen(float3 worldPos) {
				float4 projection = mul(_Projection, float4(worldPos, 1.f));
				float2 screen = projection.xy / (-projection.w * projection.z);
				screen.x /= _ScreenRatio;
				screen += .5f;
				return screen;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				float2 screenUv = WorldToScreen(vs.worldPos);
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