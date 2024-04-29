
//colors
int _colorMode;
int _colorSyncedOn;
int bitsSynced;
int bitsR;
int bitsG;
int bitsB;              
int bitsGray;
half3 grayscaleColor;
#define CM_GS 0
#define CM_RGB 1
#define CM_YIQ 2
// #define CM_YIQ2 3


//dithering
int _ditherMode;
float ditherAmount;
#define DM_0 0
#define DM_1 1
#define DM_2 2
#define DM_3 3
#define DM_4 4
#define DM_5 5
#define DM_6 6

//palette
int _ResPalette;
int paletteDelta;


//Signal Tweak
float signalAdjustY; 
float signalAdjustI; 
float signalAdjustQ; 
float signalShiftY; 
float signalShiftI; 
float signalShiftQ; 


//subshift is depricated
//p_shift to put image in the center of the quantitation 
float2 psh(){ return float2(0,0); }
//additional shift: super important
// return .5*float2(step(0., 1.-ar.y)*(1.-1./ar.y), step(1., ar.y)*(1.-ar.y));







#if VHS_DITHER

//rnd https://www.shadertoy.com/view/4djSRW
//TODO change to new version
#define MOD3 float3(443.8975,397.2973, 491.1871)
float hash12(float2 p){
   float3 p3  = frac(float3(p.xyx) * MOD3);
   p3 += dot(p3, p3.yzx + 19.19);
   return frac((p3.x + p3.y) * p3.z);
}

float ditherMask(float2 p){ //get dithering mask

   float2 p_ = p;

   //Dither modes :

   //0 No dithering
   float dm = 0; // dither mask (pattern)
           
   if( _ditherMode==DM_1 ){ //1 horizontal lines
      float pxx_y = ((p_-psh())*_Res.y+psh()).y;
      dm = floor(fmod(pxx_y, 2.))*.5;            
      // dm = floor(fmod(pxx_y, 2.)).xxx*.5;            
   }
     
   else if( _ditherMode==DM_2 ){ //2 vertical lines
      float pxx_x = ((p_.x-psh())*_Res.x+psh()).x;
      dm = floor(fmod(pxx_x, 2.))*.5;
      // dm = floor(fmod(pxx_x, 2.)).xxx*.5;
   }
    
   else if( _ditherMode==DM_3 ){ //3 2x2 ordered
      float2 pxx = (p_-psh())*_Res.xy+psh();
      float2 ij = floor(fmod(pxx, float(2.).xx));                        
      float idx = ij.x + 2.*ij.y;
      float4 m = step( abs(idx.xxxx-float4(0.,1.,2.,3.)), float(.5).xxxx ) 
         * float4(.75,.25,.0,.5);
      dm = m.x+m.y+m.z+m.w;
   }                      
    
   else if( _ditherMode==DM_4 ){ //4 2x2 ordered alternative
      float2 pxx = (p_-psh())*_Res.xy+psh(); 
      float2 s = floor( frac( floor(abs( pxx )) /2.)*2.);                      
      float f = (2.*s.x+s.y)/4.;
      dm = (f-.375); 
   }                      
   
   else if( _ditherMode==DM_5 ){ //5 uniform noise
      dm = hash12(p_);
      // dm = hash12(p_).xxx;
   }                      
    
   else if( _ditherMode==DM_6 ){ //6 triangle noise
      dm = (hash12(p_) + hash12(p_ + .59374) -.5);
      // dm = (hash12(p_) + hash12(p_ + .59374) -.5).xxx;
   }                      

   return dm;

}
#endif //VHS_DITHER



#if VHS_COLOR 

half3 rgb2yuvn(half3 c){ 
   half y = .299*c.x + .587*c.y + .114*c.z;
   c = half3(y, .5 * (c.z-y)/(1.-.114), .5 * (c.x-y)/(1.-.299) );
   c.yz += half(.5).xx;
   return c;
};

half3 yuvn2rgb(half3 c){     
   c.yz -= float(.5).xx;                      
   return half3(
      c.x + c.z*(1.-.299)/.5,
      c.x - c.y*.114*(1.-.114)/(.5*.587) - c.z*.299*(1.-.299)/(.5*.587),                        
      c.x + c.y*(1.-.114)/.5
   );
};


float3 colorDec(float3 c){

   float3 dv = float3(255,255,255);//.xxx;

   if( _colorMode==CM_GS ){
      dv = float(bitsGray).xxx;   
   }
   else if( _colorMode==CM_RGB ){
      if(_colorSyncedOn)   dv = float(bitsSynced).xxx;
      else                 dv = float3(bitsR, bitsG, bitsB);
   }
   else if( _colorMode==CM_YIQ ){
      if(_colorSyncedOn)   dv = float(bitsSynced).xxx;
      else                 dv = float3(bitsR, bitsG, bitsB);
   }

   return dv;

}


//color decimation and dithering
float3 colorEnc(float3 c, float dm){

   //amount of colors per channel
   float3 dv = float(255.).xxx;

   if( _colorMode==CM_GS ){
      dv = float(bitsGray).xxx;   
   }
   else if( _colorMode==CM_RGB ){
      if(_colorSyncedOn)   dv = float(bitsSynced).xxx;
      else                 dv = float3(bitsR, bitsG, bitsB);
   }
   else if( _colorMode==CM_YIQ ){
      if(_colorSyncedOn)   dv = float(bitsSynced).xxx;
      else                 dv = float3(bitsR, bitsG, bitsB);
   }
   // else if( _colorMode==CM_YIQ ){
   //    if(_colorSyncedOn)   dv = float(bitsSynced).xxx;
   //    else                 dv = float3(bitsR, bitsG, bitsG);
   //    // else dv = float3(bitsLuma, bitsChroma, bitsChroma);
   // }

   
   //before
   if(_colorMode==CM_YIQ) c = rgb2yuvn(c);
   if(_colorMode==CM_GS) c = (c.x+c.y+c.z).xxx/3.;

   //with rounding fix for better color matching
   //decimating colors + dither mask
   c = floor( c.xyz * dv.xyz + ditherAmount * dm.xxx ) / (dv.xyz - float(1.).xxx);
   // col = col.xyz * dv.xyz / dv.xyz;
   // if(dv.x==0.0&&dv.y==0.0&&dv.y==0.0) col = float3(1.0, 1.0, 1.0);

   //after
   if(_colorMode==CM_YIQ) c = yuvn2rgb(c);
   if(_colorMode==CM_GS) c.rgb = (c.x+c.y+c.z).xxx/3. * grayscaleColor;


   return c;

}

#endif //VHS_COLOR


#if VHS_PALETTE

half len(half3 a, half3 b){
   return abs(a.x-b.x)+abs(a.y-b.y)+abs(a.z-b.z);
}

//get palette color
half4 getPal(float pos){
   return LOAD_TEXTURE2D_X(_PaletteTex, (uint2)float2(pos,.5) ).xyzw; //need uint coz of lerp
}

half3 palette(half3 col){

   //TODO maybe sort by YIQ-Q?
   half3 mcol = col.xyz; //this is current color 
   float mc = (mcol.x+mcol.y+mcol.z)/3.; //back color

   //TODO _ResPalette/step etc. in int!
   float step = float(_ResPalette)/2.;
   float pos = step;
   float pc = 0.;

   //find closest grayscale color 
   while(step>=1.){   
      half4 cs = getPal(pos);
      pc = cs.a;//tex2D( _PaletteTex, float2(pos * ox, 0.5)).a;                          
      step /= 2.;
      if(pc>mc) pos -= step; else pos += step;                        
   }

   //adjustment
   int firstPos = pos; // for debug
   int bestPos = pos;
   
   half3 cc = getPal(pos).rgb;// tex2D( _PaletteTex, float2(pos * ox, .5)).rgb;      
   float l = len(cc, mcol);
   float nl = 0.;// new len
   float pp = 0.; //TODO int
   // for(int ii=-paletteDelta;ii<paletteDelta;ii++){ //i is input!
   float ii = -paletteDelta; while(ii<paletteDelta){ ii += 1.0;
      pp = pos+ii;
      if( pp>=0. && pp<float(_ResPalette) ){
         cc = getPal(pp).rgb;//tex2D( _PaletteTex, float2( pp * ox, .5)).rgb;
         nl = len(cc, mcol);
         if(nl<l) {
            l = nl;//wasnt here
            bestPos = pp;
         }
      }
   }
   
   col.xyz = getPal(bestPos).rgb;//tex2D( _PaletteTex, float2(bestPos * ox, .5)).rgb;
   return col;
}

#endif //VHS_PALETTE


#if VHS_SIGNAL_TWEAK_ON

   half3 signalTweak(half3 signal){

      //adjust
      signal.x += signalAdjustY; 
      signal.y += signalAdjustI; 
      signal.z += signalAdjustQ; 

      //shift
      signal.x *= signalShiftY; 
      signal.y *= signalShiftI; 
      signal.z *= signalShiftQ; 

      return signal;
   }

#endif

