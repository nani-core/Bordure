using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

using VladStorm;

// namespace VladStorm {
namespace UnityEditor.Rendering {


	[CanEditMultipleObjects]
	[CustomEditor(typeof(VHSPro))]
	// [VolumeComponentEditor(typeof(VHSPro))] //before 2022.1
	class VHSProEditor : VolumeComponentEditor {
		
		//toggles
		SerializedProperty g_pixel;
		SerializedProperty g_color;
		SerializedProperty g_palette;
		SerializedProperty g_bypass;		

		SerializedProperty g_crt;
		SerializedProperty g_noise;
		SerializedProperty g_jitter;
		SerializedProperty g_signal;
		SerializedProperty g_feedback;
		SerializedProperty g_extra;


		//screen
		SerializedDataParameter pixelOn;
		SerializedDataParameter screenResPresetId;
		SerializedDataParameter screenWidth;
		SerializedDataParameter screenHeight;


		//color
		SerializedDataParameter colorOn;
		SerializedDataParameter colorMode;
		SerializedDataParameter colorSyncedOn;
		SerializedDataParameter bitsSynced;
		SerializedDataParameter bitsR;
		SerializedDataParameter bitsG;
		SerializedDataParameter bitsB;
		SerializedDataParameter bitsGray;
		SerializedDataParameter grayscaleColor;

		//dither
		SerializedDataParameter ditherOn;
		SerializedDataParameter ditherMode;
		SerializedDataParameter ditherAmount;


		//palette
		SerializedDataParameter paletteOn;
		SerializedDataParameter paletteId;
		SerializedDataParameter paletteDelta;
		SerializedDataParameter paletteTex;


		//CRT
		SerializedDataParameter bleedOn; 
		SerializedDataParameter crtMode; 
		SerializedDataParameter bleedAmount;

		//NOISE
		SerializedDataParameter noiseResGlobal;
		SerializedDataParameter noiseResWidth;
		SerializedDataParameter noiseResHeight;		

		SerializedDataParameter filmgrainOn;
		SerializedDataParameter filmGrainAmount; 
		SerializedDataParameter tapeNoiseOn;
		SerializedDataParameter tapeNoiseTH; 
		SerializedDataParameter tapeNoiseAmount; 
		SerializedDataParameter tapeNoiseSpeed; 
		SerializedDataParameter lineNoiseOn;
		SerializedDataParameter lineNoiseAmount; 
		SerializedDataParameter lineNoiseSpeed; 


		//JITTER
		SerializedDataParameter scanLinesOn;
		SerializedDataParameter scanLineWidth;
		
		SerializedDataParameter linesFloatOn; 
		SerializedDataParameter linesFloatSpeed; 
		SerializedDataParameter stretchOn;

		SerializedDataParameter twitchHOn; 
		SerializedDataParameter twitchHFreq; 
		SerializedDataParameter twitchVOn; 
		SerializedDataParameter twitchVFreq; 

		SerializedDataParameter jitterHOn; 
		SerializedDataParameter jitterHAmount; 
		SerializedDataParameter jitterVOn; 
		SerializedDataParameter jitterVAmount; 
		SerializedDataParameter jitterVSpeed; 
		

		//SIGNAL TWEAK
		SerializedDataParameter signalTweakOn; 
		SerializedDataParameter signalAdjustY; 
		SerializedDataParameter signalAdjustI; 
		SerializedDataParameter signalAdjustQ; 

		SerializedDataParameter signalShiftY; 
		SerializedDataParameter signalShiftI; 
		SerializedDataParameter signalShiftQ; 

		SerializedDataParameter signalNoiseOn; 
		SerializedDataParameter signalNoiseAmount; 
		SerializedDataParameter signalNoisePower; 


		//FEEDBACK
		SerializedDataParameter feedbackOn; 

		SerializedDataParameter feedbackAmount; 
		SerializedDataParameter feedbackFade; 
		SerializedDataParameter feedbackColor; 	

		SerializedDataParameter feedbackThresh; 	
		SerializedDataParameter feedbackDebugOn; 	


		//TOOLS
		SerializedDataParameter independentTimeOn; 


		//bypass texture
		SerializedDataParameter bypassOn;	
		SerializedDataParameter bypassTex;	


		// public override bool hasAdvancedMode => true; //?

		public override void OnEnable() {

			base.OnEnable();

			g_pixel = 			serializedObject.FindProperty("g_pixel");
			g_color = 			serializedObject.FindProperty("g_color");
			g_palette = 		serializedObject.FindProperty("g_palette");
			g_bypass = 			serializedObject.FindProperty("g_bypass");

			g_crt = 				serializedObject.FindProperty("g_crt");
			g_noise = 			serializedObject.FindProperty("g_noise");
			g_jitter = 			serializedObject.FindProperty("g_jitter");
			g_signal = 			serializedObject.FindProperty("g_signal");
			g_feedback = 		serializedObject.FindProperty("g_feedback");
			g_extra = 			serializedObject.FindProperty("g_extra");
			g_bypass = 			serializedObject.FindProperty("g_bypass");




			var o = new PropertyFetcher<VHSPro>(serializedObject);


			//screen
			pixelOn = 				Unpack(o.Find("pixelOn"));
			screenWidth = 			Unpack(o.Find("screenWidth"));
			screenHeight = 		Unpack(o.Find("screenHeight"));

			//color 		
			colorOn = 				Unpack(o.Find("colorOn"));
			colorMode = 			Unpack(o.Find("colorMode"));
			colorSyncedOn = 		Unpack(o.Find("colorSyncedOn"));

			bitsGray = 				Unpack(o.Find("bitsGray"));
			bitsSynced = 			Unpack(o.Find("bitsSynced"));
			bitsR = 					Unpack(o.Find("bitsR"));
			bitsG = 					Unpack(o.Find("bitsG"));
			bitsB = 					Unpack(o.Find("bitsB"));
			grayscaleColor = 		Unpack(o.Find("grayscaleColor"));

			//dither
			ditherOn = 				Unpack(o.Find("ditherOn"));
			ditherMode = 			Unpack(o.Find("ditherMode"));
			ditherAmount = 		Unpack(o.Find("ditherAmount"));


			//palette
			paletteOn = 			Unpack(o.Find("paletteOn"));
			paletteId = 			Unpack(o.Find("paletteId"));
			paletteDelta = 		Unpack(o.Find("paletteDelta"));
			paletteTex = 			Unpack(o.Find("paletteTex"));


			//CRT
			bleedOn = 				Unpack(o.Find("bleedOn")); 
			crtMode = 				Unpack(o.Find("crtMode")); 
			screenResPresetId = 	Unpack(o.Find("screenResPresetId"));
			bleedAmount = 			Unpack(o.Find("bleedAmount"));


			//NOISE
			noiseResGlobal = 		Unpack(o.Find("noiseResGlobal"));
			noiseResWidth = 		Unpack(o.Find("noiseResWidth")); 
			noiseResHeight = 		Unpack(o.Find("noiseResHeight")); 

			filmgrainOn = 			Unpack(o.Find("filmgrainOn"));
			filmGrainAmount = 	Unpack(o.Find("filmGrainAmount")); 
			tapeNoiseOn = 			Unpack(o.Find("tapeNoiseOn"));
			tapeNoiseTH = 			Unpack(o.Find("tapeNoiseTH")); 
			tapeNoiseAmount = 	Unpack(o.Find("tapeNoiseAmount")); 
			tapeNoiseSpeed = 		Unpack(o.Find("tapeNoiseSpeed")); 
			lineNoiseOn = 			Unpack(o.Find("lineNoiseOn"));
			lineNoiseAmount = 	Unpack(o.Find("lineNoiseAmount")); 
			lineNoiseSpeed = 		Unpack(o.Find("lineNoiseSpeed")); 


			//JITTER
			scanLinesOn = 			Unpack(o.Find("scanLinesOn"));
			scanLineWidth = 		Unpack(o.Find("scanLineWidth"));
			
			linesFloatOn = 		Unpack(o.Find("linesFloatOn")); 
			linesFloatSpeed = 	Unpack(o.Find("linesFloatSpeed")); 
			stretchOn = 			Unpack(o.Find("stretchOn"));

			twitchHOn = 			Unpack(o.Find("twitchHOn")); 
			twitchHFreq = 			Unpack(o.Find("twitchHFreq")); 
			twitchVOn = 			Unpack(o.Find("twitchVOn")); 
			twitchVFreq = 			Unpack(o.Find("twitchVFreq")); 

			jitterHOn = 			Unpack(o.Find("jitterHOn")); 
			jitterHAmount = 		Unpack(o.Find("jitterHAmount")); 
			jitterVOn = 			Unpack(o.Find("jitterVOn")); 
			jitterVAmount = 		Unpack(o.Find("jitterVAmount")); 
			jitterVSpeed = 		Unpack(o.Find("jitterVSpeed")); 
			

			//SIGNAL TWEAK
			signalTweakOn = 		Unpack(o.Find("signalTweakOn")); 
			signalAdjustY = 		Unpack(o.Find("signalAdjustY")); 
			signalAdjustI = 		Unpack(o.Find("signalAdjustI")); 
			signalAdjustQ = 		Unpack(o.Find("signalAdjustQ")); 

			signalShiftY = 		Unpack(o.Find("signalShiftY")); 
			signalShiftI = 		Unpack(o.Find("signalShiftI")); 
			signalShiftQ = 		Unpack(o.Find("signalShiftQ")); 

			signalNoiseOn = 		Unpack(o.Find("signalNoiseOn")); 
			signalNoiseAmount = 	Unpack(o.Find("signalNoiseAmount")); 
			signalNoisePower = 	Unpack(o.Find("signalNoisePower")); 

			// gammaCorection = 		Unpack(o.Find("gammaCorection")); 


			//FEEDBACK
			feedbackOn = 			Unpack(o.Find("feedbackOn")); 

			feedbackAmount = 		Unpack(o.Find("feedbackAmount")); 
			feedbackFade = 		Unpack(o.Find("feedbackFade")); 
			feedbackColor = 		Unpack(o.Find("feedbackColor")); 

			feedbackThresh = 		Unpack(o.Find("feedbackThresh")); 
			feedbackDebugOn = 	Unpack(o.Find("feedbackDebugOn")); 


			//TOOLS
			independentTimeOn = 	Unpack(o.Find("independentTimeOn")); 
			
			//custom tex
			bypassOn = 				Unpack(o.Find("bypassOn"));
			bypassTex = 			Unpack(o.Find("bypassTex"));


		}

		public override void OnInspectorGUI() {



			GUIStyle boldFoldout = new GUIStyle(EditorStyles.foldout); // EditorStyles.miniLabel
			boldFoldout.font = EditorStyles.miniFont;
			boldFoldout.fontSize = 10;
			// boldFoldout.fontStyle = FontStyle.Bold;

			
			g_pixel.boolValue = EditorGUILayout.Foldout(g_pixel.boolValue, "Resolution", boldFoldout);
			if(g_pixel.boolValue){

				PropertyField(pixelOn, EditorGUIUtility.TrTextContent("Pixelization", ""));
      		indP();

            using (new EditorGUILayout.HorizontalScope()) { DrawOverrideCheckbox(screenResPresetId);
       		using (new EditorGUI.DisabledScope(!screenResPresetId.overrideState.boolValue)) {
				
			   	screenResPresetId.value.intValue = EditorGUILayout.Popup("Preset", screenResPresetId.value.intValue, 
			   		VHSHelper.GetResPresetNames()
			   		);
           	}}

      		if(VHSHelper.GetResPresets()[screenResPresetId.value.intValue].isCustom){
					PropertyField(screenWidth, EditorGUIUtility.TrTextContent("Width", ""));
					PropertyField(screenHeight, EditorGUIUtility.TrTextContent("Height", ""));
	      	}
	      	indM();
	      	EditorGUILayout.Space();
				
				
			}

			
			//color
			g_color.boolValue = EditorGUILayout.Foldout(g_color.boolValue, "Signal Encoding", boldFoldout); //Color Encoding & Downsampling
			if(g_color.boolValue){

				PropertyField(colorOn, EditorGUIUtility.TrTextContent("Color Encoding", ""));

					indP();
	            using (new EditorGUILayout.HorizontalScope()) { DrawOverrideCheckbox(colorMode);
	       		using (new EditorGUI.DisabledScope(!colorMode.overrideState.boolValue)) {

						colorMode.value.intValue = 	
							EditorGUILayout.Popup("Type", colorMode.value.intValue, 
								VHSHelper.colorModes);

					}}

					if(colorMode.value.intValue==0){
						
						PropertyField(bitsGray, EditorGUIUtility.TrTextContent("Channel Gray", ""));
						PropertyField(grayscaleColor, EditorGUIUtility.TrTextContent("Color", ""));

					}

					if(colorMode.value.intValue==1){

						PropertyField(colorSyncedOn, EditorGUIUtility.TrTextContent("Sync Channels", ""));
						if(colorSyncedOn.value.boolValue){
							PropertyField(bitsSynced, EditorGUIUtility.TrTextContent("All Channels", ""));
						}else{
							PropertyField(bitsR, EditorGUIUtility.TrTextContent("Channel R", ""));
							PropertyField(bitsG, EditorGUIUtility.TrTextContent("Channel G", ""));
							PropertyField(bitsB, EditorGUIUtility.TrTextContent("Channel B", ""));
						}

					}

					if(colorMode.value.intValue==2){
						PropertyField(colorSyncedOn, EditorGUIUtility.TrTextContent("Sync Channels", ""));
						if(colorSyncedOn.value.boolValue){
							PropertyField(bitsSynced, EditorGUIUtility.TrTextContent("All Channels", ""));
						}else{
							PropertyField(bitsR, EditorGUIUtility.TrTextContent("Channel Y", ""));
							PropertyField(bitsG, EditorGUIUtility.TrTextContent("Channel I", ""));
							PropertyField(bitsB, EditorGUIUtility.TrTextContent("Channel Q", ""));							
						}	
					}	
						
					indM();
					EditorGUILayout.Space();


	        
		      //SIGNAL
		   	PropertyField(signalTweakOn, EditorGUIUtility.TrTextContent("Signal Tweak", ""));

					indP(); 
					PropertyField(signalAdjustY, EditorGUIUtility.TrTextContent("Shift Y", ""));
					PropertyField(signalAdjustI, EditorGUIUtility.TrTextContent("Shift I", ""));
					PropertyField(signalAdjustQ, EditorGUIUtility.TrTextContent("Shift Q", ""));
					PropertyField(signalShiftY, EditorGUIUtility.TrTextContent("Adjust Y", ""));
					PropertyField(signalShiftI, EditorGUIUtility.TrTextContent("Adjust I", ""));
					PropertyField(signalShiftQ, EditorGUIUtility.TrTextContent("Adjust Q", ""));
				   indM();
			   	EditorGUILayout.Space();



			   PropertyField(ditherOn, EditorGUIUtility.TrTextContent("Dithering", ""));

			   	indP();
	            using (new EditorGUILayout.HorizontalScope()) { DrawOverrideCheckbox(ditherMode);
	       		using (new EditorGUI.DisabledScope(!ditherMode.overrideState.boolValue)) {

					   ditherMode.value.intValue = 	
					   	EditorGUILayout.Popup("Type", ditherMode.value.intValue, VHSHelper.ditherModes);

				   }}

				   if(ditherMode.value.intValue!=0){
						PropertyField(ditherAmount, EditorGUIUtility.TrTextContent("Amount", ""));						   	
				   }  
				   indM();
				   EditorGUILayout.Space(); 			   	

			} //color

			

			//palette
			g_palette.boolValue = EditorGUILayout.Foldout(g_palette.boolValue, "Palette", boldFoldout);
			if(g_palette.boolValue){
					
				PropertyField(paletteOn, EditorGUIUtility.TrTextContent("Enable", ""));

				indP();
            using (new EditorGUILayout.HorizontalScope()) { DrawOverrideCheckbox(paletteId);
       		using (new EditorGUI.DisabledScope(!paletteId.overrideState.boolValue)) {

					// string[] paletteNames = 		VHSHelper.GetPaletteNames();
				   paletteId.value.intValue = 
					   EditorGUILayout.Popup("Preset", paletteId.value.intValue, 
					   	VHSHelper.GetPaletteNames());
			   
			   }}

			   PropertyField(paletteDelta, EditorGUIUtility.TrTextContent("Accruacy", ""));
			   // if(VHSHelper.GetPalettes()[paletteId.value.intValue].isCustom){
			// 		PropertyField(paletteTex, EditorGUIUtility.TrTextContent("Custom Palette", ""));
			// 	}
				indM();
				EditorGUILayout.Space();

			}

	      //CRT
		   g_crt.boolValue = EditorGUILayout.Foldout(g_crt.boolValue, "CRT Emulation", boldFoldout);
		   if(g_crt.boolValue){

		   	PropertyField(bleedOn, EditorGUIUtility.TrTextContent("Bleeding", ""));
		   	
		   	indP();
            using (new EditorGUILayout.HorizontalScope()) { DrawOverrideCheckbox(crtMode);
       		using (new EditorGUI.DisabledScope(!crtMode.overrideState.boolValue)) {

				   crtMode.value.intValue = 
				   	EditorGUILayout.Popup("Type", crtMode.value.intValue,  
				   		new string[3] {"Old Three Phase", "Three Phase", "Two Phase (slow)"}); //, "Custom Curve"
				}}

			   PropertyField(bleedAmount, EditorGUIUtility.TrTextContent("Stretch", ""));	
				indM();

			}


		   //NOISE
		   g_noise.boolValue = 		EditorGUILayout.Foldout(g_noise.boolValue, "Noise", boldFoldout);
		   if(g_noise.boolValue){		   
				
      		PropertyField(noiseResGlobal, EditorGUIUtility.TrTextContent("Global Resolution", ""));
	      		indP();
		      	if(noiseResGlobal.value.boolValue==false) {
		      		PropertyField(noiseResWidth, EditorGUIUtility.TrTextContent("Width", ""));
		      		PropertyField(noiseResHeight, EditorGUIUtility.TrTextContent("Height", ""));
		      	}
	      		indM();
					EditorGUILayout.Space();	   
			   

			   PropertyField(filmgrainOn, EditorGUIUtility.TrTextContent("Film Grain", ""));
				   indP();
				   PropertyField(filmGrainAmount, EditorGUIUtility.TrTextContent("Alpha", ""));
				   indM();
				   EditorGUILayout.Space();

			  	PropertyField(signalNoiseOn, EditorGUIUtility.TrTextContent("Signal Noise", ""));
				   indP();
				   PropertyField(signalNoiseAmount, EditorGUIUtility.TrTextContent("Amount", ""));
				   PropertyField(signalNoisePower, EditorGUIUtility.TrTextContent("Power", ""));
				   indM();
				   EditorGUILayout.Space();

				PropertyField(lineNoiseOn, EditorGUIUtility.TrTextContent("Line Noise", ""));
			   	indP();
			   	PropertyField(lineNoiseAmount, EditorGUIUtility.TrTextContent("Alpha", ""));
			   	PropertyField(lineNoiseSpeed, EditorGUIUtility.TrTextContent("Speed", ""));
			   	indM();
			   	EditorGUILayout.Space();

			   PropertyField(tapeNoiseOn, EditorGUIUtility.TrTextContent("Tape Noise", ""));	
					indP();
					PropertyField(tapeNoiseTH, EditorGUIUtility.TrTextContent("Amount", ""));
					PropertyField(tapeNoiseSpeed, EditorGUIUtility.TrTextContent("Speed", ""));
					PropertyField(tapeNoiseAmount, EditorGUIUtility.TrTextContent("Alpha", ""));
				   indM();
					EditorGUILayout.Space();	   
			}


	      //JITTER
		   g_jitter.boolValue = EditorGUILayout.Foldout(g_jitter.boolValue, "Jitter & Twitch", boldFoldout);
		   if(g_jitter.boolValue){

		   	PropertyField(scanLinesOn, EditorGUIUtility.TrTextContent("Show Scanlines", ""));
				   indP();
				   PropertyField(scanLineWidth, EditorGUIUtility.TrTextContent("Width", ""));
				   indM();
					EditorGUILayout.Space();			

				PropertyField(linesFloatOn, EditorGUIUtility.TrTextContent("Floating Lines", ""));
					indP();
					PropertyField(linesFloatSpeed, EditorGUIUtility.TrTextContent("Speed", ""));
					indM();
					EditorGUILayout.Space();

				PropertyField(stretchOn, EditorGUIUtility.TrTextContent("Stretch Noise", ""));
					EditorGUILayout.Space();

				PropertyField(jitterHOn, EditorGUIUtility.TrTextContent("Interlacing", ""));
		      	indP();
		      	PropertyField(jitterHAmount, EditorGUIUtility.TrTextContent("Amount", ""));
		      	indM();
		      	EditorGUILayout.Space();

				PropertyField(jitterVOn, EditorGUIUtility.TrTextContent("Jitter", ""));		      	
		      	indP();
		      	PropertyField(jitterVAmount, EditorGUIUtility.TrTextContent("Amount", ""));
		      	PropertyField(jitterVSpeed, EditorGUIUtility.TrTextContent("Speed", ""));
			      indM();
	      		EditorGUILayout.Space();

	      	PropertyField(twitchHOn, EditorGUIUtility.TrTextContent("Twitch Horizontal", ""));
					indP();
					PropertyField(twitchHFreq, EditorGUIUtility.TrTextContent("Frequency", ""));
		      	indM();
		      	EditorGUILayout.Space();

	      	PropertyField(twitchVOn, EditorGUIUtility.TrTextContent("Twitch Vertical", ""));	
					indP();
					PropertyField(twitchVFreq, EditorGUIUtility.TrTextContent("Frequency", ""));
			      indM();
					EditorGUILayout.Space();
	   	}




		   //FEEDBACK
		   g_feedback.boolValue = EditorGUILayout.Foldout(g_feedback.boolValue, "Phosphor Trail", boldFoldout);
		   if(g_feedback.boolValue){

		   	PropertyField(feedbackOn, EditorGUIUtility.TrTextContent("Phosphor Trail", ""));

					indP();     
					PropertyField(feedbackThresh, EditorGUIUtility.TrTextContent("Input Cutoff", ""));
					PropertyField(feedbackFade, EditorGUIUtility.TrTextContent("Fade", ""));
					PropertyField(feedbackAmount, EditorGUIUtility.TrTextContent("Amount", ""));
					PropertyField(feedbackColor, EditorGUIUtility.TrTextContent("Color", ""));
					indM();
					EditorGUILayout.Space();
			}


			//TOOLS
		   g_extra.boolValue = EditorGUILayout.Foldout(g_extra.boolValue, "Tools", boldFoldout);
		   if(g_extra.boolValue){
		   	indP(); 
		   	PropertyField(independentTimeOn, EditorGUIUtility.TrTextContent("Use unscaled time", ""));
		   	PropertyField(feedbackDebugOn, EditorGUIUtility.TrTextContent("Debug Trail", ""));
		   	indM();
		   	EditorGUILayout.Space();
		   }




	      //BYPASS
			g_bypass.boolValue = EditorGUILayout.Foldout(g_bypass.boolValue, "Use Bypass Texture", boldFoldout);
			if(g_bypass.boolValue){
				indP(); 
				PropertyField(bypassOn, EditorGUIUtility.TrTextContent("Enable", ""));
				PropertyField(bypassTex, EditorGUIUtility.TrTextContent("Bypass Texture", ""));
				indM();
		   	EditorGUILayout.Space();
			}


			
		}

	   //Helpers
	   void indP(){ EditorGUI.indentLevel+=2; }
	   void indM(){ EditorGUI.indentLevel-=2; }		

	 }
}
