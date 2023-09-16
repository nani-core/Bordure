Shader "Hidden/Focus Animated Shader"
{
    Properties
    {
        _FrontColor("Front Color", Color) = (1,1,1,1)
        _BackColor ("Back Color" , Color) = (0.8, 0.8, 0.8, 1)
        _Radius ("Radius", Range(-0.5,0.5)) = 0.1
        _Width ("Width", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _FrontColor;
            float4 _BackColor;
            float _Radius;
            float _Width;

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 frag(v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float l = length(i.uv - center);
                if (abs(l - _Radius) > _Width / 2.0)
                    return fixed4(1, 1, 1, 0);
                fixed4 c = lerp(_FrontColor, _BackColor, (l - _Radius) / _Width + 0.5);
                return c;
            }
            ENDCG
        }
    }
}
