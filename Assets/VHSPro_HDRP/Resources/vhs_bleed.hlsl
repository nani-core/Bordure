//2nd VHS Pass

//uniforms
float _time;
float4 _ResOg;  //before pixelation (.xy resolution , .zw one pixel )
float4 _Res;    //after pixelation (.xy resolution , .zw one pixel )

float bleedAmount;

int feedbackOn;
int feedbackDebugOn;



half3 bms(half3 a, half3 b){  return 1.-(1.-a)*(1.-b); }

half3 rgb2yiq(half3 c){   
  return half3(
    (0.2989*c.x + 0.5959*c.y + 0.2115*c.z),
    (0.5870*c.x - 0.2744*c.y - 0.5229*c.z),
    (0.1140*c.x - 0.3216*c.y + 0.3114*c.z)
  );
};

half3 yiq2rgb(half3 c){       
  return half3(
    (  1.0*c.x +    1.0*c.y +   1.0*c.z),
    ( 0.956*c.x - 0.2720*c.y - 1.1060*c.z),
    (0.6210*c.x - 0.6474*c.y + 1.7046*c.z)
  );
};


half3 t2d(float2 p){
   half3 col = LOAD_TEXTURE2D_X(_InputTexture, (uint2)(p*_ScreenSize.xy) ).xyz; //output color
   return rgb2yiq( col );
}

#define p_fix (p - float2( .5*_One.x, 0.)) 

//main
half3 vhs2( Varyings input ){

   float2 p = input.texcoord; // normalized tex coordnates 0..1  
   float t = _time;          

   float2 _One = _ResOg.zw;

   //TODO if bleedon     
   //Signal Filters values by Hans-Kristian Arntzen (luma, chroma filters)
   int bleedLength; 
   #if VHS_OLD_THREE_PHASE //(OLD_THREE_PHASE)
      bleedLength = 25;
      float LF[25]; LF[0]=-.000071070; LF[1]=-.000032816; LF[2]=.000128784; LF[3]=.000134711; LF[4]=-.000226705; LF[5]=-.000777988; LF[6]=-.000997809; LF[7]=-.000522802; LF[8]=.000344691; LF[9]=.000768930; LF[10]=.000275591; LF[11]=-.000373434; LF[12]=.000522796; LF[13]=.003813817; LF[14]=.007502825; LF[15]=.006786001; LF[16]=-.002636726; LF[17]=-.019461182; LF[18]=-.033792479; LF[19]=-.029921972; LF[20]=.005032552; LF[21]=.071226466; LF[22]=.151755921; LF[23]=.218166470; LF[24]=.243902439;
      float CF[25]; CF[0]=.001845562; CF[1]=.002381606; CF[2]=.003040177; CF[3]=.003838976; CF[4]=.004795341; CF[5]=.005925312; CF[6]=.007242534; CF[7]=.008757043; CF[8]=.010473987; CF[9]=.012392365; CF[10]=.014503872; CF[11]=.016791957; CF[12]=.019231195; CF[13]=.021787070; CF[14]=.024416251; CF[15]=.027067414; CF[16]=.029682613; CF[17]=.032199202; CF[18]=.034552198; CF[19]=.036677005; CF[20]=.038512317; CF[21]=.040003044; CF[22]=.041103048; CF[23]=.041777517; CF[24]=.042004791;
   #elif VHS_THREE_PHASE //(THREE_PHASE) //https://github.com/gizmo98/common-shaders/blob/master/ntsc/ntsc-decode-filter-3phase.inc
      bleedLength = 25;
      float LF[25]; LF[0]=-.000012020; LF[1]=-.000022146; LF[2]=-.000013155; LF[3]=-.000012020; LF[4]=-.000049979; LF[5]=-.000113940; LF[6]=-.000122150; LF[7]=-.000005612; LF[8]=.000170516; LF[9]=.000237199; LF[10]=.000169640; LF[11]=.000285688; LF[12]=.000984574; LF[13]=.002018683; LF[14]=.002002275; LF[15]=-.000909882; LF[16]=-.007049081; LF[17]=-.013222860; LF[18]=-.012606931; LF[19]=.002460860; LF[20]=.035868225; LF[21]=.084016453; LF[22]=.135563500; LF[23]=.175261268; LF[24]=.190176552;
      float CF[25]; CF[0]=-.000118847; CF[1]=-.000271306; CF[2]=-.000502642; CF[3]=-.000930833; CF[4]=-.001451013; CF[5]=-.002064744; CF[6]=-.002700432; CF[7]=-.003241276; CF[8]=-.003524948; CF[9]=-.003350284; CF[10]=-.002491729; CF[11]=-.000721149; CF[12]=.002164659; CF[13]=.006313635; CF[14]=.011789103; CF[15]=.018545660; CF[16]=.026414396; CF[17]=.035100710; CF[18]=.044196567; CF[19]=.053207202; CF[20]=.061590275; CF[21]=.068803602; CF[22]=.074356193; CF[23]=.077856564; CF[24]=.079052396;
   #elif VHS_TWO_PHASE //(TWO_PHASE) //https://github.com/gizmo98/common-shaders/blob/master/ntsc/ntsc-decode-filter-2phase.inc
      bleedLength = 33;
      float LF[33]; LF[0]=-.000174844; LF[1]=-.000205844; LF[2]=-.000149453; LF[3]=-.000051693; LF[4]=.000000000; LF[5]=-.000066171; LF[6]=-.000245058; LF[7]=-.000432928; LF[8]=-.000472644; LF[9]=-.000252236; LF[10]=.000198929; LF[11]=.000687058; LF[12]=.000944112; LF[13]=.000803467; LF[14]=.000363199; LF[15]=.000013422; LF[16]=.000253402; LF[17]=.001339461; LF[18]=.002932972; LF[19]=.003983485; LF[20]=.003026683; LF[21]=-.001102056; LF[22]=-.008373026; LF[23]=-.016897700; LF[24]=-.022914480; LF[25]=-.021642347; LF[26]=-.008863273; LF[27]=.017271957; LF[28]=.054921920; LF[29]=.098342579; LF[30]=.139044281; LF[31]=.168055832; LF[32]=.178571429;
      float CF[33]; CF[0]=.001384762; CF[1]=.001678312; CF[2]=.002021715; CF[3]=.002420562; CF[4]=.002880460; CF[5]=.003406879; CF[6]=.004004985; CF[7]=.004679445; CF[8]=.005434218; CF[9]=.006272332; CF[10]=.007195654; CF[11]=.008204665; CF[12]=.009298238; CF[13]=.010473450; CF[14]=.011725413; CF[15]=.013047155; CF[16]=.014429548; CF[17]=.015861306; CF[18]=.017329037; CF[19]=.018817382; CF[20]=.020309220; CF[21]=.021785952; CF[22]=.023227857; CF[23]=.024614500; CF[24]=.025925203; CF[25]=.027139546; CF[26]=.028237893; CF[27]=.029201910; CF[28]=.030015081; CF[29]=.030663170; CF[30]=.031134640; CF[31]=.031420995; CF[32]=.031517031;
   #else
      float LF[1];
      float CF[1];
   #endif



   //Bleeding
   half3 signal = half3(0,0,0);

   #if VHS_BLEED_ON
    
      half3 norm =  half3(0,0,0);
      half3 adj =   half3(0,0,0);

      int taps = bleedLength-4;
      for (int i = 0; i < taps % 1023; i++){

         float2 off = float2( (float(i)-float(taps))*_One.x*bleedAmount, 0.);
         half3 sums =   t2d(p_fix + off) +
                        t2d(p_fix - off);

         adj = half3(LF[i+3], CF[i], CF[i]);

         signal += sums * adj;
         norm += adj;
          
      }


      adj = half3(LF[taps], CF[taps], CF[taps]);


      signal += t2d(p_fix) * adj;
      norm += adj;
      signal = signal / norm;
    
   #else
      //no bleeding
      signal = t2d(p);
   #endif

    
   half3 col = yiq2rgb(signal);          
   

   if(feedbackOn==1){
      half3 fbb = LOAD_TEXTURE2D_X(_FeedbackTex, (uint2)(p*_ScreenSize.xy) ).xyz; //output color
      col = bms(col, fbb*1.);       
   }
   if(feedbackDebugOn==1){
      half3 fbb = LOAD_TEXTURE2D_X(_FeedbackTex, (uint2)(p*_ScreenSize.xy) ).xyz; //output color
      col = fbb;      
   }


   return col;


}
