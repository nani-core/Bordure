//VHS First Pass



//uniforms
float2 _OneOg; //one pixel in og coord system
float2 _OneN; //one pixel noise

//Filmgrain, Linenoise, Tapenoise
float tapeNoiseAmount;

//YIQ Noise
float signalNoisePower;
float signalNoiseAmount;

//Twitch, Linesfloat
float linesFloatSpeed;

//Scanlines
float scanLineWidth;

//Stretch
float twitchHFreq;
float twitchVFreq;

//Jitter
float jitterHAmount;
float jitterVAmount; 
float jitterVSpeed;



//Tools And Noise Functions

//blend mode screen
half3 bms(half3 a, half3 b){ return 1.-(1.-a)*(1.-b); }
half bms(half a, half b){ return 1.-(1.-a)*(1.-b); }

//turns sth on and off //a - how often 
float onOff(float a, float b, float c, float t){
   return step(c, sin(t + a*cos(t*b)));
}


#if defined(VHS_YIQNOISE_ON)

//this hash for 0..1 values
#define MOD3 float3(443.8975,397.2973, 491.1871)
float2 hash22(float2 p) {
   float3 p3 = frac(float3(p.xyx) * MOD3);
   p3 += dot(p3.zxy, p3.yzx+19.19);
   return frac(float2((p3.x + p3.y)*p3.z, (p3.x+p3.z)*p3.y));
}

//yiq noise by iq
//different behavior - for multiplication for example 
float2 n4rand_bw( float2 p, float t, float c ){
     
   t = frac( t );//that's why its sort of twitching 
   float2 nrnd0 = hash22( p + .07*t );
   c = 1./ (10.*c); 
   nrnd0 = pow(nrnd0, c);           
   return nrnd0; //TODO try to invert 1-...
}

half3 noiseYIQ(half3 signal, float2 pn, float t){
   //TODO make cheaper noise             
   //type 1 (best) w Y mask
   float2 pn_ = pn*_ResOg.xy;

   float2 noise = n4rand_bw( pn_,t, 1.-signalNoisePower ) ; 
   signal.y += (noise.x*2.-1.)*signalNoiseAmount*signal.x;
   signal.z += (noise.y*2.-1.)*signalNoiseAmount*signal.x;

   //type 2
   // float2 noise = n4rand_bw( pn_,t, 1.0-signalNoisePower ) ; 
   // signal.y += (noise.x*2.0-1.0)*signalNoiseAmount;
   // signal.z += (noise.y*2.0-1.0)*signalNoiseAmount;

   //type 3
   // float2 noise = n4rand_bw( pn_,t, 1.0-signalNoisePower )*signalNoiseAmount ; 
   // signal.y *= noise.x;
   // signal.z *= noise.y;
   // signal.x += (noise.x*2.0-1.0)*0.05;

   return signal;
}
#endif


#if defined(VHS_JITTER_V_ON)

float rnd_rd(float2 co){
   float a = 12.9898;
   float b = 78.233;
   float c = 43758.5453;
   float dt= dot(co.xy ,float2(a,b));
   float sn= fmod(dt,3.14);
   return frac(sin(sn) * c);
}

#endif

half3 rgb2yiq(half3 c){   
   return half3(
      (.2989*c.x + .5959*c.y + .2115*c.z),
      (.5870*c.x - .2744*c.y - .5229*c.z),
      (.1140*c.x - .3216*c.y + .3114*c.z)
   );
};

half3 yiq2rgb(half3 c){       
   return half3(
      (  1.0*c.x +    1.0*c.y +   1.0*c.z),
      ( 0.956*c.x - 0.2720*c.y - 1.1060*c.z),
      (0.6210*c.x - 0.6474*c.y + 1.7046*c.z)
   );
};



#if defined(VHS_SCANLINES_ON)

   //lines floating down           
   float scanLines(float2 p, float t){
     
      //cheap (maybe make an option later)
      // float scanLineWidth = 0.26;
      // float scans = 0.5*(cos((p.y*screenLinesNum+t+.5)*2.0*PI) + 1.0);
      // if(scans>scanLineWidth) scans = 1.; else scans = 0.;               

      float t_sl = 0.;             
      //if lines aren't floating -> scanlines also shudn't 
      //TODO bool instead of feature
      #if defined(VHS_LINESFLOAT_ON)
        t_sl = t*linesFloatSpeed;
      #endif
       
      //expensive but better                
      float scans = .5*(cos( (p.y*_Res.y+t_sl)*2.*PI) + 1.);
      scans = pow(scans, scanLineWidth); 
      scans = 1.-scans;
      return scans; 

      //TODO maybe with smoothstep is cheaper
      // float scans = .5*(cos( (p.y*screenLinesNum+t_sl)*2.0*PI) + 1.0);
   }       

#endif


#if defined(VHS_STRETCH_ON)

float gcos(float2 uv, float s, float p){
   return (cos( uv.y*PI*2.*s + p)+1.)*.5;
}

//mw - maximum width
//wcs = widthChangeSpeed
//lfs = line float speed = .5
//lf phase = line float phase = .0
float2 stretch(float2 p, float t, float mw, float wcs, float lfs, float lfp){  
     
   //width change
   float tt = t*wcs; //widthChangeSpeed
   float t2 = tt-fmod(tt, 0.5);
    
   //float dw  = hash42( vec2(0.01, t2) ).x ; //on t and not on y
   float w = gcos(p, 2.0*(1.0-frac(t2)), PI-t2) * clamp( gcos(p, frac(t2), t2) , .5, 1.);
   //w = clamp(w,0.,1.);
   w = floor(w*mw)/mw;
   w *= mw;
   //get descreete line number
   float ln = (1.-frac(t*lfs + lfp))*_Res.y;// screenLinesNum; 
   ln = ln - frac(ln); 
    
   //stretching part
    
   // float oy = 1.0/SLN; //TODO global
   float md = fmod(ln, w); //shift within the wide line 0..width
   float sh2 =  1.0-md/w; // shift 0..1

     
   float slb = _Res.y / w; //screen lines big        
   
   //TODO check if OneOg? or One
   if(p.y<_OneOg.y*ln && p.y>_OneOg.y*(ln-w)) ////if(p.y>oy*ln && p.y<oy*(ln+w)) 
      p.y = floor( p.y*slb +sh2 )/slb - (sh2-1.0)/slb ;

   return p;
}

#endif  


#if defined(VHS_JITTER_V_ON)

//yiq distortion //m - amount //returns yiq value
float3 jitterV(float2 p, float m, float t){

   m *= 0.0001; // float m = 0.0009;
   float3 offx = p.xxx; //float3( p.x, p.x, p.x );  offset x
   offx.r += rnd_rd(float2(t*.03, p.y*.42)) * .001 + sin(rnd_rd(float2(t*.2, p.y)))*m;
   offx.g += rnd_rd(float2(t*.004,p.y*.002)) * .004 + sin(t*9.)*m;

   half3 signal = half3(
      rgb2yiq( LOAD_TEXTURE2D_X(_InputTexture, float2(offx.r, p.y)*_ScreenSize.xy ).rgb ).x,
      rgb2yiq( LOAD_TEXTURE2D_X(_InputTexture, float2(offx.g, p.y)*_ScreenSize.xy ).rgb ).y,
      rgb2yiq( LOAD_TEXTURE2D_X(_InputTexture, float2(offx.b, p.y)*_ScreenSize.xy ).rgb ).z
   );
   // signal.x = rgb2yiq( tex2D( _MainTex, float2(offsetX.r, uv.y) ).rgb ).x;
   // signal.y = rgb2yiq( tex2D( _MainTex, float2(offsetX.g, uv.y) ).rgb ).y;
   // signal.z = rgb2yiq( tex2D( _MainTex, float2(offsetX.b, uv.y) ).rgb ).z;

   // signal = yiq2rgb(col);
   return signal;
     
}

#endif

#if defined(VHS_JITTER_H_ON)

float2 jitterH(float2 p, float t){

   //interlacing. (thanks to @xra)
   if( fmod( p.y * _Res.y, 2.)<1.) 
      p.x += _OneOg.x*sin(t*13000.)*jitterHAmount;

   return p;
}

#endif


#if defined(VHS_TWITCH_V_ON)

   //shift part of the screen sometimes with freq
   float2 twitchVertical(float freq, float2 p, float t){
      float vShift = .4*onOff(freq,3.,.9, t);
      vShift*=(sin(t)*sin(t*20.) + (.5 + 0.1*sin(t*200.)*cos(t)));
      p.y = fmod(p.y + vShift, 1.); 
   return p;
 }

#endif

#if defined(VHS_TWITCH_H_ON) 

   //twitches the screen //freq - how often 
   float2 twitchHorizonal(float freq, float2 p, float t){
     
      float window = 1./(1.+20.*(p.y-fmod(t/4.,1.))*(p.y-fmod(t/4., 1.)));
         p.x += sin(p.y*10. + t)/50.
         *onOff(freq,4.,.3, t)
         *(1.+cos(t*80.))
         *window;
    
      return p;
   }

#endif

#if defined(VHS_TAPENOISE_ON)

   float distShift; // for 2nd part of tape noise
   float2 tapeNoiseUV(float2 p, float2 pn){


      //uv distortion part of tapenoise
      int distWidth = 20; 
      float distAmount = 4.;
      float distThreshold = .55;
      distShift = 0; // for 2nd part of tape noise 
      for (int ii = 0; ii < distWidth % 1023; ii++){

        //this is t.n. line value at pn.y and down each pixel
        //TODO i guess ONEXN shud be 1.0/sln noise              
        float tnl = LOAD_TEXTURE2D_X(_TapeTex, float2(0., pn.y-_OneN.x*ii)*_ResN.xy ).y;
        // float tnl = tex2Dlod(_TapeTex, float4(0., pn.y-_OneN.x*ii, 0., 0.)).y;
        // float tnl = tex2Dlod(_TapeTex, float4(0.0,pn.y-ONEXN*ii, 0.0, 0.0)).y;
        // float tnl = tex2D(_TapeTex, float2(0.0,pn.y-ONEXN*ii)).y;
        // float tnl = tapeNoiseLines(float2(0.0,pn.y-ONEXN*i), t*tapeNoiseSpeed)*tapeNoiseAmount;

        // float fadediff = hash12(float2(pn.x-ONEXN*i,pn.y)); 
        if(tnl>distThreshold) {             
          //TODO get integer part other way
          float sh = sin( PI*(float(ii)/float(distWidth))) ; //0..1               
          p.x -= float( int(sh) * distAmount * _OneN.x ); //displacement
          // p.x -= float(int(sh)*distAmount*ONEXN); //displacement
          distShift += sh ; //for 2nd part
          // p.x +=  ONEXN * float(int(((tnl-thth)/thth)*distAmount));
          // col.x = sh;  
        }

      }
      
      return p;
   }

   //2nd part with noise, tail and yiq col shift
   half3 tapeNoiseCol(half3 signal, half4 colN, float2 pn){


      //here is normilized p (0..1)
      half tn = colN.x; //v2.0
      // half tn = tex2D(_TapeTex, pn).x;
      signal.x = bms(signal.x, tn*tapeNoiseAmount );  
      // float tn = tapeNoise(pn, t*tapeNoiseSpeed)*tapeNoiseAmount;

      //tape noise tail
      int tailLength = 10; //TODO adjustable
      for(int j=0; j<tailLength % 1023; j++){

         float jj = float(j);
         float2 d = float2(pn.x-_OneN.x*jj, pn.y);
         float4 colN2 = LOAD_TEXTURE2D_X(_TapeTex, d*_ResN.xy ).xyzw; //v2.0
         tn = colN2.x; //v2.0
         // tn = tex2Dlod(_TapeTex, float4(d,0,0) ).x;
         // float2 d = float2(pn.x-ONEXN*jj,pn.y);
         // tn = tex2D(_TapeTex, d).x;
         // tn = tapeNoise(float2(pn.x-ONEXN*i,pn.y), t*tapeNoiseSpeed)*tapeNoiseAmount;

         //for tails length difference
         float fadediff = colN2.a; //v2.0
         // float fadediff = tex2D(_TapeTex, d).a; //hash12(d); 

         if( tn > .8 ){               
            float nsx = 0.; //new signal x
            float newlength = float(tailLength)*(1.-fadediff); //tail lenght diff
            if( jj <= newlength ) nsx = 1.-( jj/ newlength ); //tail
            signal.x = bms(signal.x, nsx*tapeNoiseAmount);                  
         }
        
      }

      //tape noise color shift
      if(distShift>.4){
         // float tnl = tapeNoiseLines(float2(0.0,pn.y), t*tapeNoiseSpeed)*tapeNoiseAmount;
         float tnl = colN.y;//v2.0
         // float tnl = tex2D(_TapeTex, pn).y;//tapeNoiseLines(float2(0.0,pn.y), t*tapeNoiseSpeed)*tapeNoiseAmount;
         signal.y *= 1./distShift;//tnl*0.1;//*distShift;//*signal.x;
         signal.z *= 1./distShift;//*distShift;//*signal.x;

      }

      return signal;

   }

#endif


//MAIN
half3 vhs( Varyings input ){

   float t = _time;
   float2 po = input.texcoord; // og normalized tex coordnates 0..1  
   float2 p = po;    //main p     
   float2 pn = po;   //noise p

   _OneOg = _ResOg.zw;
   _OneN = _ResN.zw;

   // return LOAD_TEXTURE2D_X(_PaletteTex, (uint2)(p*float2(_ResPalette,1)) ).xyz; //need uint coz of lerp


   //UV process part

   #if defined(VHS_TWITCH_V_ON)    
      p = twitchVertical(.5*twitchVFreq, p, t); 
   #endif  

   #if defined(VHS_TWITCH_H_ON)
      p = twitchHorizonal(.1*twitchHFreq, p, t);
   #endif  

   //make discrete lines with or without float 
   float sh = 0.; //float shift
   #if defined(VHS_LINESFLOAT_ON)
      sh = frac(t*linesFloatSpeed); //shift  // float sh = fmod(t, 1.); //shift
   #endif
   p.x = floor( p.x*_Res.x )/_Res.x;
   p.y = floor( p.y*_Res.y - sh )/_Res.y - sh/_Res.y;  //v2
   // p.y = -floor( -p.y*_Res.y + sh )/_Res.y + sh/_Res.y;  //v2

   pn.x = floor( pn.x*_ResN.x )/_ResN.x;
   pn.y = floor( pn.y*_ResN.y + sh )/_ResN.y - sh/_ResN.y;
   

   #if defined(VHS_STRETCH_ON)
      p = stretch(p, t, 15., 1., .5, 0.);
      p = stretch(p, t, 8., 1.2, .45, .5);
      p = stretch(p, t, 11., .5, -.35, .25); //up   
   #endif

   #if defined(VHS_JITTER_H_ON)
      p = jitterH(p, t);
   #endif

   #if defined(VHS_TAPENOISE_ON)
      p = tapeNoiseUV(p, pn);
   #endif  

   //signal proccess part

   half3 col = half3(0,0,0);
   half3 signal = half3(0,0,0);  //signal has negative values

   #if defined(VHS_JITTER_V_ON)          
      signal = jitterV(p, jitterVAmount, t*jitterVSpeed);
   #else

      col = LOAD_TEXTURE2D_X(_InputTexture, (uint2)(p*_ScreenSize.xy) ).xyz; //output color
      signal = rgb2yiq(col);

   #endif

   half4 colN = half4(0,0,0,0);
   #if defined(VHS_TAPENOISE_ON) || defined(VHS_LINENOISE_ON) || defined(VHS_FILMGRAIN_ON) 
      colN = LOAD_TEXTURE2D_X(_TapeTex, pn*_ResN.xy ).xyzw;      
   #endif

   #if defined(VHS_LINENOISE_ON) || defined(VHS_FILMGRAIN_ON)
      signal.x += colN.z;
      // signal.x += tex2D(_TapeTex, pn).z;
   #endif
    
   #if defined(VHS_YIQNOISE_ON)
      signal = noiseYIQ(signal, pn, t);
   #endif
   
   #if defined(VHS_TAPENOISE_ON)
      signal = tapeNoiseCol(signal, colN, pn);
   #endif

   #if defined(VHS_SIGNAL_TWEAK_ON)
      signal = signalTweak(signal);
   #endif

   //color process part
   col = yiq2rgb(signal); //signal to rgb color space


   float dm = 0.; //dithering mask
   #if defined(VHS_DITHER)  
      dm = ditherMask(p); 
   #endif

   //amount of colors per channel
   float3 dv = float3(255,255,255);//.xxx;
   #if defined(VHS_COLOR)
      dv = colorDec(col); //color decimation      
   #endif


   #if defined(VHS_COLOR) || defined(VHS_DITHER)
      
      //before
      #if defined(VHS_COLOR)
      if(_colorMode==CM_GS) col = (col.x+col.y+col.z).xxx/3.;
      if(_colorMode==CM_YIQ) col = rgb2yuvn(col);
      #endif

      //with rounding fix for better color matching
      //decimating colors + dither mask
      col = floor( col.xyz * dv.xyz + ditherAmount * dm.xxx ) / (dv.xyz - float(1.).xxx);

      //after
      #if defined(VHS_COLOR)
      if(_colorMode==CM_GS) col.rgb = (col.x+col.y+col.z).xxx/3. * grayscaleColor;
      if(_colorMode==CM_YIQ) col = yuvn2rgb(col);
      #endif

   #endif

   #if defined(VHS_PALETTE)
      col = palette(col);
   #endif 

   //TODO put it into 2nd pass?
   #if defined(VHS_SCANLINES_ON)
      col *= scanLines(po, t);             
   #endif


   return col.xyz;

}

