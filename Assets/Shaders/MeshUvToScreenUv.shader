Shader "NaniCore/MeshUvToScreenUv" {
	SubShader {
		Tags {
			"RenderType" = "Opaque"
			"RenderPipeline" = "HighDefinitionRenderPipeline"
		}

		Pass {
			Cull Off
			ZTest LEqual
			ZWrite On

			HLSLINCLUDE
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassRenderers.hlsl"
			ENDHLSL

			HLSLPROGRAM
			#pragma vertex vertex_shader
			#pragma fragment pixel_shader
			#pragma exclude_renderers nomrt
			#pragma target 3.0
 
			struct structureVS {
				float4 vertex : SV_POSITION;	// Vertex position.
				float2 uv : TEXCOORD0;			// UV on the mesh, which in this case is the actual rendered position.
			};
 
			struct structurePS {
				float4 target00 : SV_Target0;
			};
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;
				float4 clipPos = TransformObjectToHClip(vertex.xyz);	// Clip-space coordinate.
				vs.uv = uv;
				float2 meshUvAsScreenPos = (uv - .5f) / .5f;	// Mesh UV normalized for screen.
				meshUvAsScreenPos.y = -meshUvAsScreenPos.y;
				vs.vertex = float4(meshUvAsScreenPos, 1, 1);
				return vs;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;
				ps.target00 = float4(vs.uv, 0, 1);
				return ps;
			}
			ENDHLSL
		}
	}
}
