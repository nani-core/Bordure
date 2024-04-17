Shader "Hidden/VHSPro_feedback"{

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
   TEXTURE2D_X(_LastTex);
   TEXTURE2D_X(_FeedbackTex);          

   float feedbackAmount;
   float feedbackFade;
   float feedbackThresh;
   half3 feedbackColor;



   half3 bms(half3 a, half3 b){  return 1.-(1.-a)*(1.-b); }
   half grey(half3 a){  return (a.x+a.y+a.z)/3.; }

   half len(half3 a, half3 b){
      return (abs(a.x-b.x)+abs(a.y-b.y)+abs(a.z-b.z))/3.;
   }


   float4 CustomPostProcess(Varyings input) : SV_Target {

      UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

      float2 p = input.texcoord; // og normalized tex coordnates 0..1  
      float one_x = 1./_ScreenParams.x;

      //new feedback value
      half3 fc = LOAD_TEXTURE2D_X(_InputTexture, (uint2)(p*_ScreenSize.xy) ).xyz; 
      half3 fl = LOAD_TEXTURE2D_X(_LastTex, (uint2)(p*_ScreenSize.xy) ).xyz; 

      // return half4(fl, 0.);
      // return half4(grey(saturate(fc-fl)).xxx, 0.);
      // half3 fc =  tex2D( _MainTex, i.uvn).rgb;     //current frame without feedback
      // half3 fl =  tex2D( _LastTex, i.uvn).rgb;     //last frame without feedback
      float diff = grey(saturate(fc-fl)); //dfference between frames
      // float diff = len(fc,fl); //dfference between frames
      // float diff = len(fl,fc); //dfference between frames
      // float diff = abs(fl.x-fc.x + fl.y-fc.y + fl.z-fc.z)/3.; //dfference between frames
      if(diff<feedbackThresh) diff = 0.;

      half3 fbn = fc*diff*feedbackAmount; //feedback new
      // half3 fbn = fc*diff*feedbackAmount; //feedback new
      // fbn = half3(0.0, 0.0, 0.0);
      

      //old feedback buffer
      half3 fbb = ( //blur old buffer a bit
         LOAD_TEXTURE2D_X(_FeedbackTex, (uint2)((p)*_ScreenSize.xy) ).xyz *.5 +
         LOAD_TEXTURE2D_X(_FeedbackTex, (uint2)((p+ float2(one_x,0))*_ScreenSize.xy) ).xyz *.25 +
         LOAD_TEXTURE2D_X(_FeedbackTex, (uint2)((p- float2(one_x,0))*_ScreenSize.xy) ).xyz *.25 
      );// / 3.;
      fbb *= feedbackFade;
      // if( (fbb.x+fbb.y+fbb.z)/3.<.01 ) fbb = half3(0,0,0);

      fbn = bms(fbn, fbb); 

      return float4(fbn * feedbackColor, 1.); //*feedbackColor 

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
