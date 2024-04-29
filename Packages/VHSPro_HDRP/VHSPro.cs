using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

using VladStorm;

[Serializable, VolumeComponentMenu("Post-processing/VHS Pro")]
public sealed class VHSPro : CustomPostProcessVolumeComponent, IPostProcessComponent {

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;


    //Toggles
    public bool g_pixel = true;
    public bool g_color = true;
    public bool g_palette = true;

    public bool g_crt = true;
    public bool g_noise = true;
    public bool g_jitter = true;
    public bool g_signal = true;
    public bool g_feedback = true;
    public bool g_extra = false;
    public bool g_bypass = false;   


    //Screen
    public BoolParameter            pixelOn = new BoolParameter(false);
    public IntParameter             screenResPresetId = new IntParameter(2);
    public NoInterpIntParameter     screenWidth = new NoInterpIntParameter(640);
    public NoInterpIntParameter     screenHeight = new NoInterpIntParameter(480);

    //Color encoding
    public BoolParameter            colorOn = new BoolParameter(false);
    public IntParameter             colorMode = new IntParameter(2);
    public BoolParameter            colorSyncedOn = new BoolParameter(true);
    public ClampedIntParameter      bitsR = new ClampedIntParameter(2,0,255);
    public ClampedIntParameter      bitsG = new ClampedIntParameter(2,0,255);
    public ClampedIntParameter      bitsB = new ClampedIntParameter(2,0,255);
    public ClampedIntParameter      bitsSynced = new ClampedIntParameter(2,0,255); 
    public ClampedIntParameter      bitsGray = new ClampedIntParameter(1,0,255);
    public ColorParameter           grayscaleColor = new ColorParameter(Color.white);

    //Dither
    public BoolParameter            ditherOn = new BoolParameter(false);
    public NoInterpIntParameter     ditherMode = new NoInterpIntParameter(3);
    public ClampedFloatParameter    ditherAmount = new ClampedFloatParameter(1f, -1f, 3f);

    //Palette
    public BoolParameter            paletteOn = new BoolParameter(false);
    public NoInterpIntParameter     paletteId = new NoInterpIntParameter(0);
    public ClampedIntParameter      paletteDelta = new ClampedIntParameter(5,0,30);
    public TextureParameter         paletteTex = new TextureParameter(null);
    // PalettePreset paletteCustom; 
    // string paletteCustomName = ""; //for automatic update when drag and drop texture
    // bool paletteCustomInit = false; 

    //crt
    public BoolParameter bleedOn  = new BoolParameter(false); 
    public NoInterpIntParameter crtMode = new NoInterpIntParameter(0); 
    public ClampedFloatParameter bleedAmount  = new ClampedFloatParameter(1f, 0f, 5f);


    //Noise
    public BoolParameter noiseResGlobal  = new BoolParameter(true); 
    public NoInterpIntParameter noiseResWidth = new NoInterpIntParameter(640);
    public NoInterpIntParameter noiseResHeight = new NoInterpIntParameter(480);

    public BoolParameter filmgrainOn  = new BoolParameter(false);
    public ClampedFloatParameter filmGrainAmount = new ClampedFloatParameter(0.016f, 0f, 1f); 

    public BoolParameter signalNoiseOn  = new BoolParameter(false);
    public ClampedFloatParameter signalNoiseAmount = new ClampedFloatParameter(0.3f, 0f, 1f);
    public ClampedFloatParameter signalNoisePower  = new ClampedFloatParameter(0.83f, 0f, 1f);

    public BoolParameter lineNoiseOn  = new BoolParameter(false);
    public ClampedFloatParameter lineNoiseAmount = new ClampedFloatParameter(1f, 0f, 10f);
    public ClampedFloatParameter lineNoiseSpeed = new ClampedFloatParameter(5f, 0f, 10f);

    public BoolParameter tapeNoiseOn  = new BoolParameter(false);
    public ClampedFloatParameter tapeNoiseTH = new ClampedFloatParameter(0.63f, 0f, 1.5f);
    public ClampedFloatParameter tapeNoiseAmount = new ClampedFloatParameter(1f, 0f, 1.5f); 
    public ClampedFloatParameter tapeNoiseSpeed = new ClampedFloatParameter(1f, 0f, 1.5f);

    //Jitter
    public BoolParameter scanLinesOn  = new BoolParameter(false);
    public ClampedFloatParameter scanLineWidth = new ClampedFloatParameter(10f,0f,20f);
    
    public BoolParameter linesFloatOn  = new BoolParameter(false); 
    public ClampedFloatParameter linesFloatSpeed = new ClampedFloatParameter(1f,-3f,3f);
    public BoolParameter stretchOn  = new BoolParameter(false);

    public BoolParameter jitterHOn  = new BoolParameter(false);
    public ClampedFloatParameter jitterHAmount = new ClampedFloatParameter(.5f,0f,5f);
    public BoolParameter jitterVOn  = new BoolParameter(false); 
    public ClampedFloatParameter jitterVAmount = new ClampedFloatParameter(1f,0f,15f);
    public ClampedFloatParameter jitterVSpeed = new ClampedFloatParameter(1f,0f,5f);

    public BoolParameter twitchHOn  = new BoolParameter(false);
    public ClampedFloatParameter twitchHFreq = new ClampedFloatParameter(1f,0f,5f);
    public BoolParameter twitchVOn  = new BoolParameter(false);
    public ClampedFloatParameter twitchVFreq = new ClampedFloatParameter(1f,0f,5f);
    
    //Signal Tweak
    public BoolParameter signalTweakOn  = new BoolParameter(false); 
    public ClampedFloatParameter signalAdjustY = new ClampedFloatParameter(0f,-0.25f, 0.25f);
    public ClampedFloatParameter signalAdjustI = new ClampedFloatParameter(0f,-0.25f, 0.25f);
    public ClampedFloatParameter signalAdjustQ = new ClampedFloatParameter(0f,-0.25f, 0.25f);
    public ClampedFloatParameter signalShiftY = new ClampedFloatParameter(1f,-2f, 2f);
    public ClampedFloatParameter signalShiftI = new ClampedFloatParameter(1f,-2f, 2f);
    public ClampedFloatParameter signalShiftQ = new ClampedFloatParameter(1f,-2f, 2f);

    //Feedback
    public BoolParameter feedbackOn  = new BoolParameter(false); 
    public ClampedFloatParameter feedbackThresh = new ClampedFloatParameter(.1f, 0f, 1f);
    public ClampedFloatParameter feedbackAmount = new ClampedFloatParameter(2.0f, 0f, 3f);  
    public ClampedFloatParameter feedbackFade = new ClampedFloatParameter(.82f, 0f, 1f);
    public ColorParameter feedbackColor = new ColorParameter(new Color(1f,.5f,0f)); 
    public BoolParameter feedbackDebugOn  = new BoolParameter(false); 
    public int feedbackMode = 0; 

    //Tools 
    public BoolParameter independentTimeOn  = new BoolParameter(false); 
    float _time = 0f;

    //Bypass     
    public BoolParameter            bypassOn = new BoolParameter(false);  
    public TextureParameter         bypassTex = new TextureParameter(null);



    //Materials, Textures and RTH
    Material mat1;               //1st pass
    Material matBleed;           //2nd pass vhs bleeding + mix with feedback
    Material matTape;            //tape noise
    Material matFeedback;        //feedback

    RTHandle tex1;               //first pass output
    RTHandle texTape;            //tape noise texture
    RTHandle texFeedback;        //feedback buffer
    RTHandle texFeedbackLast;    //feedback prev frame
    RTHandle texLast;            //prev frame
    RTHandle texBypass;

    Vector2Int camRes = Vector2Int.zero; //camera size current
    Vector2Int camResPrev = Vector2Int.zero; //camera size previous - to track screen size change

    // Vector2Int texTape_size =   Vector2Int.zero; 
    // Vector2Int texFeedback_size = Vector2Int.zero; 
    // Vector2Int texBypass_size = Vector2Int.zero; 

    public bool IsActive(){

        //everything is off by default
        if(pixelOn.value==false &&
            colorOn.value==false &&
            ditherOn.value==false &&
            paletteOn.value==false &&
            bleedOn.value==false &&
            filmgrainOn.value==false &&
            signalNoiseOn.value==false &&
            lineNoiseOn.value==false &&
            tapeNoiseOn.value==false &&
            scanLinesOn.value==false &&
            linesFloatOn.value==false &&
            jitterHOn.value==false &&
            jitterVOn.value==false &&
            twitchHOn.value==false &&
            twitchVOn.value==false &&
            signalTweakOn.value==false &&
            feedbackOn.value==false &&
            bypassOn.value==false) {
            return false;
        }

        return true;
    } 


    

    public override void Setup() {        
    }

    void InitTextures(Vector2Int camRes, bool resChanged){ //HDCamera camera //camWidth, int camHeight, Vector4 _ResN, 

        //the strategy is to create/clean up all the materials and textures once 
        //otherwise (create/clean up on every frame) didnt work (didnt show vfx in the editor scenario)        

        //we need to use material references which are saved in the resources folder 
        //we switching these materials keywords -> they will be later included in the build 

        if(mat1==null)          LoadMat(ref mat1,          "Materials/VHSPro_pass1");
        if(matTape==null)       LoadMat(ref matTape,       "Materials/VHSPro_tape");
        if(matBleed==null)      LoadMat(ref matBleed,      "Materials/VHSPro_bleed");
        if(matFeedback==null)   LoadMat(ref matFeedback,   "Materials/VHSPro_feedback");

        //lets setup textures only at the start or when resolution is changed
        if(tex1==null || resChanged){
            RTHandles.Release(tex1);
            tex1 = InitRTH(camRes);
        }

        if(texTape==null || resChanged){
            if(tapeNoiseOn.value || filmgrainOn.value || lineNoiseOn.value){
                RTHandles.Release(texTape);
                texTape = InitRTH(camRes);
            }
        }

        if(texFeedback==null || resChanged){
            if(feedbackOn.value){
                RTHandles.Release(texFeedback);
                RTHandles.Release(texFeedbackLast);
                RTHandles.Release(texLast);
                texFeedback = InitRTH(camRes);
                texFeedbackLast = InitRTH(camRes);
                texLast = InitRTH(camRes);
            }
        }

        if(texBypass==null || resChanged){
            if(bypassOn.value){
                RTHandles.Release(texBypass);
                texBypass = InitRTH(camRes);
            }
        }


    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination) {
        
        //skip in scene view
        if(camera.camera.cameraType==CameraType.SceneView) {
            // Graphics.Blit(source, destination);
            return;
        }

        //init palettes and resolutions
        VHSHelper.Init();

        //calulations 
        if(independentTimeOn.value){ _time = Time.unscaledTime; }
        else{                        _time = Time.time; }

        //camera resolution actually used for rendering after dynamic resolution and XR is applied.
        camRes = new Vector2Int(camera.actualWidth, camera.actualHeight);  


        //original screen resolution (.xy resolution .zw one pixel)
        Vector4 _ResOg = new Vector4(camRes.x, camRes.y, 1f/camRes.x, 1f/camRes.y); 


        ResPreset resPreset = VHSHelper.GetResPresets()[screenResPresetId.value];
        if(resPreset.isCustom!=true){
            screenWidth.value  = resPreset.screenWidth;
            screenHeight.value = resPreset.screenHeight;
        }
        if(resPreset.isFirst==true || pixelOn.value==false){
            screenWidth.value  = camRes.x; //camera.actualWidth;
            screenHeight.value = camRes.y; //camera.actualHeight;
        }

        //resolution after pixelation
        Vector4 _Res = new Vector4(screenWidth.value, screenHeight.value, 0f,0f);
        _Res[2] = 1f/_Res.x;                                    
        _Res[3] = 1f/_Res.y;                                    

        //noise resolution
        Vector4 _ResN = new Vector4(_Res.x, _Res.y, _Res.z, _Res.w);
        if(!noiseResGlobal.value){
            _ResN = new Vector4(noiseResWidth.value, noiseResHeight.value, 0f, 0f);
            _ResN[2] = 1f/_ResN.x;                                    
            _ResN[3] = 1f/_ResN.y;                                                
        }

        //lets track if camera resolution changed -> we will need to init all textures again 
        bool resChanged = false;
        if(camResPrev!=Vector2Int.zero)
        if(camRes.x!=camResPrev.x || camRes.y!=camResPrev.y ) {
            resChanged = true;
        }

        //we need to create textures/materials at the start and when resolution changed
        InitTextures(camRes, resChanged); 

        //lets save prev cam resolution
        camResPrev = new Vector2Int(camRes.x, camRes.y); 


        //PASS PART

        //screen params
        mat1.SetFloat("_time",      _time);  
        mat1.SetVector("_ResOg",    _ResOg);
        mat1.SetVector("_Res",      _Res);
        mat1.SetVector("_ResN",      _ResN);

        //Pixelation
        //...

        //Color decimat1ion
        FeatureToggle(mat1, colorOn.value, "VHS_COLOR");        
        mat1.SetInt("_colorMode",                colorMode.value);
        mat1.SetInt("_colorSyncedOn",            colorSyncedOn.value?1:0);

        mat1.SetInt("bitsR",                     bitsR.value);
        mat1.SetInt("bitsG",                     bitsG.value);
        mat1.SetInt("bitsB",                     bitsB.value);
        mat1.SetInt("bitsSynced",                bitsSynced.value);

        mat1.SetInt("bitsGray",                  bitsGray.value);
        mat1.SetColor("grayscaleColor",          grayscaleColor.value);        

        FeatureToggle(mat1, ditherOn.value, "VHS_DITHER");        
        mat1.SetInt("_ditherMode",            ditherMode.value);
        mat1.SetFloat("ditherAmount",         ditherAmount.value);


        //Signal Tweak
        FeatureToggle(mat1, signalTweakOn.value, "VHS_SIGNAL_TWEAK_ON");

        mat1.SetFloat("signalAdjustY", signalAdjustY.value);
        mat1.SetFloat("signalAdjustI", signalAdjustI.value);
        mat1.SetFloat("signalAdjustQ", signalAdjustQ.value);

        mat1.SetFloat("signalShiftY", signalShiftY.value);
        mat1.SetFloat("signalShiftI", signalShiftI.value);
        mat1.SetFloat("signalShiftQ", signalShiftQ.value);


        //Palette
        FeatureToggle(mat1, paletteOn.value, "VHS_PALETTE");

        PalettePreset pal = VHSHelper.GetPalettes()[paletteId.value];
        mat1.SetTexture("_PaletteTex",   pal.texSorted);
        mat1.SetInt("_ResPalette",       pal.texSortedWidth);

        mat1.SetInt("paletteDelta",           paletteDelta.value);
        

        //VHS 1st Pass (Distortions, Decimations)
        FeatureToggle(mat1, filmgrainOn.value, "VHS_FILMGRAIN_ON");
        FeatureToggle(mat1, tapeNoiseOn.value, "VHS_TAPENOISE_ON");
        FeatureToggle(mat1, lineNoiseOn.value, "VHS_LINENOISE_ON");

        
        //Jitter & Twitch
        FeatureToggle(mat1, jitterHOn.value, "VHS_JITTER_H_ON");
        mat1.SetFloat("jitterHAmount", jitterHAmount.value);

        FeatureToggle(mat1, jitterVOn.value, "VHS_JITTER_V_ON");
        mat1.SetFloat("jitterVAmount", jitterVAmount.value);
        mat1.SetFloat("jitterVSpeed", jitterVSpeed.value);

        FeatureToggle(mat1, linesFloatOn.value, "VHS_LINESFLOAT_ON");     
        mat1.SetFloat("linesFloatSpeed", linesFloatSpeed.value);

        FeatureToggle(mat1, twitchHOn.value, "VHS_TWITCH_H_ON");
        mat1.SetFloat("twitchHFreq", twitchHFreq.value);

        FeatureToggle(mat1, twitchVOn.value, "VHS_TWITCH_V_ON");
        mat1.SetFloat("twitchVFreq", twitchVFreq.value);

        FeatureToggle(mat1, scanLinesOn.value, "VHS_SCANLINES_ON");
        mat1.SetFloat("scanLineWidth", scanLineWidth.value);
        
        FeatureToggle(mat1, signalNoiseOn.value, "VHS_YIQNOISE_ON");
        mat1.SetFloat("signalNoisePower", signalNoisePower.value);
        mat1.SetFloat("signalNoiseAmount", signalNoiseAmount.value);

        FeatureToggle(mat1, stretchOn.value, "VHS_STRETCH_ON");


        //Noises
        if(tapeNoiseOn.value || filmgrainOn.value || lineNoiseOn.value){

            matTape.SetFloat("_time", _time);  

            FeatureToggle(matTape, filmgrainOn.value, "VHS_FILMGRAIN_ON");
            matTape.SetFloat("filmGrainAmount", filmGrainAmount.value);
            
            FeatureToggle(matTape, tapeNoiseOn.value, "VHS_TAPENOISE_ON");
            matTape.SetFloat("tapeNoiseTH", tapeNoiseTH.value);
            matTape.SetFloat("tapeNoiseAmount", tapeNoiseAmount.value);
            matTape.SetFloat("tapeNoiseSpeed", tapeNoiseSpeed.value);
            
            FeatureToggle(matTape, lineNoiseOn.value, "VHS_LINENOISE_ON");
            matTape.SetFloat("lineNoiseAmount", lineNoiseAmount.value);
            matTape.SetFloat("lineNoiseSpeed", lineNoiseSpeed.value);

            HDUtils.DrawFullScreen(cmd, matTape, texTape);       
            
            mat1.SetTexture("_TapeTex",         texTape);
            mat1.SetFloat("tapeNoiseAmount", tapeNoiseAmount.value);          

        }


        //VHS 2nd Pass (Bleed)
        matBleed.SetFloat("_time",  _time);  
        matBleed.SetVector("_ResOg", _ResOg);//  - resolution before pixelation
        matBleed.SetVector("_Res",   _Res);//  - resolution after pixelation

        //CRT       
        FeatureToggle(matBleed, bleedOn.value, "VHS_BLEED_ON");

        matBleed.DisableKeyword("VHS_OLD_THREE_PHASE");
        matBleed.DisableKeyword("VHS_THREE_PHASE");
        matBleed.DisableKeyword("VHS_TWO_PHASE");           
              if(crtMode.value==0){ matBleed.EnableKeyword("VHS_OLD_THREE_PHASE"); }
        else if(crtMode.value==1){ matBleed.EnableKeyword("VHS_THREE_PHASE"); }
        else if(crtMode.value==2){ matBleed.EnableKeyword("VHS_TWO_PHASE"); }

        matBleed.SetFloat("bleedAmount", bleedAmount.value);


        //1st pass
        //Bypass Texture
        if(bypassOn.value){
            Graphics.Blit(bypassTex.value, texBypass.rt); //TODO maybe copy texture instead of graph.blit
            mat1.SetTexture("_InputTexture", texBypass);   
        }else{
            mat1.SetTexture("_InputTexture", source);
        }
        HDUtils.DrawFullScreen(cmd, mat1, tex1);    



        if(feedbackOn.value){
 
            //recalc feedback buffer
            matFeedback.SetFloat("feedbackThresh",   feedbackThresh.value);
            matFeedback.SetFloat("feedbackAmount",   feedbackAmount.value);
            matFeedback.SetFloat("feedbackFade",     feedbackFade.value);
            matFeedback.SetColor("feedbackColor",    feedbackColor.value);

            matFeedback.SetTexture("_InputTexture",      tex1);
            matFeedback.SetTexture("_LastTex",           texLast);
            matFeedback.SetTexture("_FeedbackTex",       texFeedbackLast);

            HDUtils.DrawFullScreen(cmd, matFeedback, texFeedback); //texFeedback2

            cmd.CopyTexture(texFeedback, texFeedbackLast);  //save prev frame feedback
            cmd.CopyTexture(tex1, texLast);             //save prev frame color
        
        }

        matBleed.SetInt("feedbackOn",            feedbackOn.value?1:0);
        matBleed.SetInt("feedbackDebugOn",       feedbackDebugOn.value?1:0);
        if(feedbackOn.value || feedbackDebugOn.value){
            matBleed.SetTexture("_FeedbackTex",      texFeedback);
        }

        //2nd pass
        matBleed.SetTexture("_InputTexture",     tex1);
        HDUtils.DrawFullScreen(cmd, matBleed, destination);  


    }


    public override void Cleanup(){

        //RTH
        RTHandles.Release(tex1);
        RTHandles.Release(texTape);
        RTHandles.Release(texFeedback);
        RTHandles.Release(texFeedbackLast);
        RTHandles.Release(texLast);
        RTHandles.Release(texBypass);

    }

    //Helper Tools
    void FeatureToggle(Material mat, bool propVal, string featureName){  //turn on/off shader features
        if(propVal)     mat.EnableKeyword(featureName);
        else            mat.DisableKeyword(featureName);
    }

    void LoadMat(ref Material m, string materialPath){      
        m = Resources.Load<Material>(materialPath);
        if(m==null) 
            Debug.LogError($"Unable to find material '{materialPath}'. Post-Process Volume VHSPro is unable to load.");
    }


    /*
    void LoadShader(ref Shader shader, string shaderName){
        shader = Shader.Find(shaderName);
        if( shader==null )  Debug.LogError($"Unable to find shader '{shaderName}'. Post Process Volume VHSPro is unable to load.");
    }


    void LoadMat(ref Material m, Shader shader){
        CoreUtils.Destroy(m);
        m = new Material(shader); 
        //Shader.Find(shaderName)
        // if( Shader.Find(shaderName)!=null )             
        // else 
        //     Debug.LogError($"Unable to find shader '{shaderName}'. Post Process Volume VHSPro is unable to load.");
    }
    */

    //inits default RTH
    RTHandle InitRTH(Vector2Int camRes){

        return RTHandles.Alloc(
            camRes.x, 
            camRes.y,
            TextureXR.slices, 
            filterMode: FilterMode.Point, 
            dimension: TextureXR.dimension, 
            useDynamicScale: true, 
            enableRandomWrite: true                
        );

    }


}


