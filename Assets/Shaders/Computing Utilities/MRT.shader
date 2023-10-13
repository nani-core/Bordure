Shader "NaniCore/MRT" {
	Properties {
		_Value ("Value", Color) = (1, 1, 1, 1)
	}
	SubShader {
		Tags {
			"RenderType" = "Opaque"
			"RenderPipeline" = "HighDefinitionRenderPipeline"
		}

		Pass {
			Cull Off
			ZTest LEqual
			ZWrite Off

			HLSLINCLUDE
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassRenderers.hlsl"

			float4 _Value;
			ENDHLSL

			HLSLPROGRAM
			#pragma vertex vertex_shader
			#pragma fragment pixel_shader
			#pragma exclude_renderers nomrt
			#pragma target 3.0
 
			struct structureVS {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
 
			struct structurePS {
				float4 target00 : SV_Target0;
			};
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				vs.vertex = TransformObjectToHClip(vertex.xyz);
				vs.uv = uv;
				return vs;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				ps.target00 = _Value;
				return ps;
			}
			ENDHLSL
		}
	}
}
