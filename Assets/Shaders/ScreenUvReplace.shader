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
			#include "UnityCG.cginc"
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
			float4x4 _WorldToCam;
			float4x4 _CameraProjection;
			float4x4 _WhereToWorld;
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				vs.vertex = UnityObjectToClipPos(vertex);
				vs.uv = uv;
				vs.worldPos = mul(_WhereToWorld, vs.vertex);
				return vs;
			}

			float3 WorldToCamera(float3 worldPos) {
				float4 camera = mul(_WorldToCam, float4(worldPos, 1.f));
				return camera.xyz / camera.w;
			}

			float2 WorldToScreen(float3 worldPos) {
				float3 camera = WorldToCamera(worldPos);
				float4 screen = mul(_CameraProjection, float4(camera, 1.f));
				screen /= screen.z;
				// screen = screen * .5f + .5f;
				return screen.xy;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				float2 screenUv = WorldToScreen(vs.worldPos);
				if(true) {
					float4 camera = mul(_WorldToCam, float4(vs.worldPos, 1.f));
					ps.target00 = float4(screenUv.xy, abs(camera.z) / 10, 1);
					return ps;
				}
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