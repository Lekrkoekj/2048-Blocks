Shader "Unlit/UILoopingTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _TextureSize("Texture Size", Float) = 1
        _TextureOpacity("Texture Opacity", Range(0, 1)) = 0.1
        _TextureMoveSpeed("Texture Scrolling Speed", Float) = 1
        [HideInInspector] _GlobalTime("GlobalTime", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _TextureSize;
            float _TextureOpacity;
            float _TextureMoveSpeed;
            float _GlobalTime;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                float aspectRatio = _ScreenParams.x / _ScreenParams.y;

                screenUV.x *= aspectRatio;
                screenUV *= _TextureSize;
                screenUV.y += _GlobalTime * _TextureMoveSpeed;

                screenUV = frac(screenUV);

                fixed4 col = tex2D(_MainTex, screenUV);

                col *= _TextureOpacity;

                col += _Color;

                return col;
            }
            ENDCG
        }
    }
}
