Shader "NaniCore/MeshUvToScreenUv" {
	SubShader {
		Tags {
			"RenderType" = "Opaque"
			"RenderPipeline" = "HighDefinitionRenderPipeline"
		}

		Pass {
			Cull Back
			ZTest LEqual
			ZWrite Off

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
				float4 clipPos : TEXCOORD1;
			};
 
			struct structurePS {
				float4 target00 : SV_Target0;
			};

			float _Fov;
 
			structureVS vertex_shader(float4 vertex: POSITION, float2 uv: TEXCOORD0) {
				structureVS vs;

				vs.clipPos = TransformObjectToHClip(vertex.xyz);	// Clip-space coordinate.

				vs.uv = uv;

				float2 meshUvAsScreenPos = (uv - .5f) / .5f;	// Mesh UV normalized for screen.
				meshUvAsScreenPos.y = -meshUvAsScreenPos.y;

				vs.vertex = float4(meshUvAsScreenPos, 1, 1);
				// Debug
				if(false) {
					vs.vertex = TransformObjectToHClip(vertex.xyz);
				}
				return vs;
			}
 
			structurePS pixel_shader(structureVS vs) {
				structurePS ps;

				float verticalUnit = tan(_Fov * .5f * atan(1.f) / 45.f);

				float2 uv = vs.clipPos.xy / (vs.clipPos.z * vs.clipPos.w);

				uv = uv / 5;	// Where does this 5 come?
				uv.y *= -1;
				uv = uv * .5f + .5f;
				ps.target00 = float4(uv, 0, 1);
				return ps;
			}
			ENDHLSL
		}
	}
}
