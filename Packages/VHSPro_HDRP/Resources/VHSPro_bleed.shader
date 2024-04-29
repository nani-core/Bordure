﻿Shader "Hidden/VHSPro_bleed"{

   HLSLINCLUDE

   #pragma target 4.5
   #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
   #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
   #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
   #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"


   struct Attributes {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
   };

   struct Varyings {
      float4 positionCS : SV_POSITION;
      float2 texcoord   : TEXCOORD0;
      UNITY_VERTEX_OUTPUT_STEREO
   };


   Varyings Vert(Attributes input) {
      Varyings output;
      UNITY_SETUP_INSTANCE_ID(input);
      UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
      output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
      output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
      return output;
   }

   // List of properties to control your post process effect
   TEXTURE2D_X(_InputTexture);
   TEXTURE2D_X(_FeedbackTex);


   #pragma shader_feature_local_fragment _ VHS_BLEED_ON
   #pragma shader_feature_local_fragment _ VHS_OLD_THREE_PHASE
   #pragma shader_feature_local_fragment _ VHS_THREE_PHASE
   #pragma shader_feature_local_fragment _ VHS_TWO_PHASE

   #pragma shader_feature_local_fragment _ VHS_SIGNAL_TWEAK_ON
   #include "vhs_bleed.hlsl" 


   float4 CustomPostProcess(Varyings input) : SV_Target {

      UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

      float3 outColor = vhs2(input);
      return float4(outColor, 1);
      
   }

   ENDHLSL

   SubShader {

        Pass {
            Name "VHSPro"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
   }
   Fallback Off
}
