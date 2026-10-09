Shader "UI/SpriteOutlineTrail"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,1,1,1)
        _Thickness ("Thickness (Textur-Pixel)", Float) = 6
        _Phase ("Phase (vom Script)", Float) = 0 
        _Tail ("Tail Length", Range(0.05, 1)) = 0.4
        _AlphaCutoff ("Alpha Cutoff", Range(0.05, 0.95)) = 0.5   
        _CenterUV ("Center UV", Vector) = (0.5, 0.5, 0, 0)
        _Intensity ("Intensity", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
               "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 local : TEXCOORD1;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color, _OutlineColor;
            float _Thickness, _Phase, _Tail, _AlphaCutoff, _Intensity;
            float4 _CenterUV;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            float Inside(float2 uv)
            {
                float a = tex2D(_MainTex, uv).a;
                return smoothstep(_AlphaCutoff - 0.1, _AlphaCutoff + 0.1, a);  
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float inside = Inside(i.uv);
                float2 texel = _MainTex_TexelSize.xy;

                float minIn = inside;
                [unroll] for (int r = 1; r <= 3; r++)
                {
                    float rad = _Thickness * (r / 3.0);
                    [unroll] for (int k = 0; k < 12; k++)
                    {
                        float ang = k * 0.5235988;
                        float2 d = float2(cos(ang), sin(ang));
                        minIn = min(minIn, Inside(i.uv + d * texel * rad));
                    }
                }
                float edge = inside * (1.0 - minIn);

                float2 p = (i.uv - _CenterUV.xy) / _MainTex_TexelSize.xy;
                float angle = atan2(p.y, p.x);
                float s = frac(-angle / 6.2831853);          

                float dist = frac(_Phase - s);           
                float trail = dist < _Tail ? 1.0 - dist / _Tail : 0.0;

                float m = edge * trail * _Intensity;
                return fixed4(_OutlineColor.rgb, _OutlineColor.a * m * i.color.a);
            }
            ENDCG
        }
    }
}