Shader "GPE/GlitchEffectShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Texture Color", Color) = (1, 1, 1, 1)

        [Space(20)] [Header(Chromatic Aberration Settings)]        
        [Space(5)] _ChromaticAmount ("Chromatic Aberration Amount", Range(-0.5, 0.5)) = 0
        _ChromaticTextureMultiplier("Chromatic Aberration Texture Strength", Range(0,1)) = 0
        _TransparencyAmount ("Transparency Amount", Range(0, 1)) = 0.5

        [Space(20)] [Header(Noise Settings)]
        [Space(5)] _NoiseMap ("Noise Map", 2D) = "black" {}
        _NoiseStrength("Noise Strength", Range(-1, 1)) = 0
        _NoiseSpeed("Noise Movement Speed", Float) = 0
        _NoiseSize("Noise Map Size", Range(0.01, 1)) = 0
        _MovementSmoothing("Move Noise per How Many Seconds?", Float) = 0

        [Space(20)] [Header(Pixel Displacement Settings)]
        [Space(5)] _Row1Tex ("Rows Texture", 2D) = "black" {}
        _RowSize("Row Size", Float) = 1
        _Offset("Offset", Range(0, 0.5)) = 0
        _RowMoveSpeed("Row Movement Speed", Float) = 0
        _DisplacementMovementSmoothing("Move Displacement per How Many Seconds?", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        // Object_W
        Pass
        {
            Name "Object"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
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

            // Chromatic Aberration
            float _ChromaticAmount;
            float _ChromaticTextureMultiplier;

            // Noise
            sampler2D _NoiseMap;
            float _NoiseStrength;
            float _NoiseSpeed;
            float _NoiseSize;
            float _MovementSmoothing;

            // Pixel Displacement
            sampler2D _Row1Tex;
            float _RowSize;
            float _Offset;
            float _RowMoveSpeed;
            float _DisplacementMovementSmoothing;

            v2f vert (appdata v)
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex.x -= _Offset;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Main Texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                
                // Texture Chromatic Aberration
                float2 redUv = i.uv -= float2(_ChromaticAmount * _ChromaticTextureMultiplier / 2, 0);
                float2 greenUv = i.uv += float2(0, _ChromaticAmount * _ChromaticTextureMultiplier / 2);
                float2 blueUv = i.uv += float2(_ChromaticAmount * _ChromaticTextureMultiplier / 2, 0);

                float4 redCol = tex2D(_MainTex, redUv);
                float4 greenCol = tex2D(_MainTex, greenUv);
                float4 blueCol = tex2D(_MainTex, blueUv);

                fixed4 combinedCol = tex2D(_MainTex, i.uv);

                combinedCol.r = redCol.r;
                combinedCol.g = greenCol.g;
                combinedCol.b = blueCol.b;

                col = col * (1 - _ChromaticTextureMultiplier) + combinedCol * _ChromaticTextureMultiplier;

                // Noise
                float2 screenUV = i.screenPos.xy / i.screenPos.w * _NoiseSize;

                float2 noiseUv = screenUV;

                float movementTime = round(_Time.y * _MovementSmoothing) / _MovementSmoothing;
                noiseUv = screenUV + float2(movementTime * _NoiseSpeed, movementTime * _NoiseSpeed);
                fixed4 noiseCol = tex2D(_NoiseMap, noiseUv);
                
                col.r = col.r + noiseCol.r * _NoiseStrength;
                col.r = clamp(col.r, 0, 1);
                col.g = col.g + noiseCol.g * _NoiseStrength;
                col.g = clamp(col.g, 0, 1);
                col.b = col.b + noiseCol.b * _NoiseStrength;
                col.b = clamp(col.b, 0, 1);

                // Pixel Displacement
                float3 objectPosition = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                float3 cameraPosition = _WorldSpaceCameraPos;
                float distanceToCamera = distance(objectPosition, cameraPosition);
                float adjustedRowSize = _RowSize * distanceToCamera;

                float displacementMovementTime = round(_Time.y * _DisplacementMovementSmoothing) / _DisplacementMovementSmoothing;
                screenUV = i.screenPos.xy / i.screenPos.w * adjustedRowSize + displacementMovementTime * _RowMoveSpeed;

                fixed4 rowOverlay = tex2D(_Row1Tex, screenUV);

                col.a = (rowOverlay.r + rowOverlay.b + rowOverlay.g) / 3;

                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }

        // Object_B
        Pass
        {
            Name "Object"
            Blend SrcAlpha OneMinusSrcAlpha
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

            // Chromatic Aberration
            float _ChromaticAmount;
            float _ChromaticTextureMultiplier;

            // Noise
            sampler2D _NoiseMap;
            float _NoiseStrength;
            float _NoiseSpeed;
            float _NoiseSize;
            float _MovementSmoothing;

            // Pixel Displacement
            sampler2D _Row1Tex;
            float _RowSize;
            float _Offset;
            float _RowMoveSpeed;
            float _DisplacementMovementSmoothing;

            v2f vert (appdata v)
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex.x += _Offset;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Main Texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                
                // Texture Chromatic Aberration
                float2 redUv = i.uv -= float2(_ChromaticAmount * _ChromaticTextureMultiplier / 2, 0);
                float2 greenUv = i.uv += float2(0, _ChromaticAmount * _ChromaticTextureMultiplier / 2);
                float2 blueUv = i.uv += float2(_ChromaticAmount * _ChromaticTextureMultiplier / 2, 0);

                float4 redCol = tex2D(_MainTex, redUv);
                float4 greenCol = tex2D(_MainTex, greenUv);
                float4 blueCol = tex2D(_MainTex, blueUv);

                fixed4 combinedCol = tex2D(_MainTex, i.uv);

                combinedCol.r = redCol.r;
                combinedCol.g = greenCol.g;
                combinedCol.b = blueCol.b;

                col = col * (1 - _ChromaticTextureMultiplier) + combinedCol * _ChromaticTextureMultiplier;

                // Noise
                float2 screenUV = i.screenPos.xy / i.screenPos.w * _NoiseSize;

                float2 noiseUv = screenUV;
                float movementTime = round(_Time.y * _MovementSmoothing) / _MovementSmoothing;
                noiseUv = screenUV + float2(movementTime * _NoiseSpeed, movementTime * _NoiseSpeed);
                fixed4 noiseCol = tex2D(_NoiseMap, noiseUv);
                
                col.r = col.r + noiseCol.r * _NoiseStrength;
                col.r = clamp(col.r, 0, 1);
                col.g = col.g + noiseCol.g * _NoiseStrength;
                col.g = clamp(col.g, 0, 1);
                col.b = col.b + noiseCol.b * _NoiseStrength;
                col.b = clamp(col.b, 0, 1);

                // Pixel Displacement
                float3 objectPosition = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                float3 cameraPosition = _WorldSpaceCameraPos;
                float distanceToCamera = distance(objectPosition, cameraPosition);
                float adjustedRowSize = _RowSize * distanceToCamera;

                float displacementMovementTime = round(_Time.y * _DisplacementMovementSmoothing) / _DisplacementMovementSmoothing;
                screenUV = i.screenPos.xy / i.screenPos.w * adjustedRowSize + displacementMovementTime * _RowMoveSpeed;

                fixed4 rowOverlay = tex2D(_Row1Tex, screenUV);

                col.a = 1 - (rowOverlay.r + rowOverlay.b + rowOverlay.g) / 3;

                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }

        // Red_W
        Pass
        {
            Name "Red_Channel_W"
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
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
            float4 _Color;

            // Chromatic Aberration
            float4 _MainTex_ST;
            float _ChromaticAmount;
            float _TransparencyAmount;

            // Noise
            sampler2D _NoiseMap;
            float _NoiseStrength;
            float _NoiseSpeed;
            float _NoiseSize;
            float _MovementSmoothing;

            // Pixel Displacement
            sampler2D _Row1Tex;
            float _RowSize;
            float _Offset;
            float _RowMoveSpeed;
            float _DisplacementMovementSmoothing;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex.x += _ChromaticAmount + _Offset;
                o.vertex.z -= 0.02;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Main Texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;

                // Noise
                float2 screenUV = i.screenPos.xy / i.screenPos.w * _NoiseSize;

                float2 noiseUv = screenUV;
                float movementTime = round(_Time.y * _MovementSmoothing) / _MovementSmoothing;
                noiseUv = screenUV + float2(movementTime * _NoiseSpeed, movementTime * _NoiseSpeed);
                fixed4 noiseCol = tex2D(_NoiseMap, noiseUv);
                
                col.r = col.r + noiseCol.r * _NoiseStrength * 5;
                col.r = clamp(col.r, 0, 1);

                // Chromatic Aberration Red Color
                col.b = 0;
                col.g = 0;
                col.a = col.r * _TransparencyAmount;

                // Pixel Displacement
                float3 objectPosition = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                float3 cameraPosition = _WorldSpaceCameraPos;
                float distanceToCamera = distance(objectPosition, cameraPosition);
                float adjustedRowSize = _RowSize * distanceToCamera;

                float displacementMovementTime = round(_Time.y * _DisplacementMovementSmoothing) / _DisplacementMovementSmoothing;
                screenUV = i.screenPos.xy / i.screenPos.w * adjustedRowSize + displacementMovementTime * _RowMoveSpeed;

                fixed4 rowOverlay = tex2D(_Row1Tex, screenUV);

                col.a *= rowOverlay.r;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        // Red_B
        Pass
        {
            Name "Red_Channel_B"
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
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
            float4 _Color;

            // Chromatic Aberration
            float4 _MainTex_ST;
            float _ChromaticAmount;
            float _TransparencyAmount;

            // Noise
            sampler2D _NoiseMap;
            float _NoiseStrength;
            float _NoiseSpeed;
            float _NoiseSize;
            float _MovementSmoothing;

            // Pixel Displacement
            sampler2D _Row1Tex;
            float _RowSize;
            float _Offset;
            float _RowMoveSpeed;
            float _DisplacementMovementSmoothing;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex.x += _ChromaticAmount - _Offset;
                o.vertex.z -= 0.02;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Main Texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;

                // Noise
                float2 screenUV = i.screenPos.xy / i.screenPos.w * _NoiseSize;

                float2 noiseUv = screenUV;
                float movementTime = round(_Time.y * _MovementSmoothing) / _MovementSmoothing;
                noiseUv = screenUV + float2(movementTime * _NoiseSpeed, movementTime * _NoiseSpeed);
                fixed4 noiseCol = tex2D(_NoiseMap, noiseUv);
                
                col.r = col.r + noiseCol.r * _NoiseStrength;
                col.r = clamp(col.r, 0, 1);

                // Chromatic Aberration Red Color
                col.b = 0;
                col.g = 0;
                col.a = col.r * _TransparencyAmount;

                // Pixel Displacement
                float3 objectPosition = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                float3 cameraPosition = _WorldSpaceCameraPos;
                float distanceToCamera = distance(objectPosition, cameraPosition);
                float adjustedRowSize = _RowSize * distanceToCamera;

                float displacementMovementTime = round(_Time.y * _DisplacementMovementSmoothing) / _DisplacementMovementSmoothing;
                screenUV = i.screenPos.xy / i.screenPos.w * adjustedRowSize + displacementMovementTime * _RowMoveSpeed;

                fixed4 rowOverlay = tex2D(_Row1Tex, screenUV);

                rowOverlay = 1 - rowOverlay;

                col.a *= rowOverlay.r;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        // Green_W
        Pass
        {
            Name "Green_Channel_W"
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
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
            float4 _Color;

            // Chromatic Aberration
            float4 _MainTex_ST;
            float _ChromaticAmount;
            float _TransparencyAmount;

            // Noise
            sampler2D _NoiseMap;
            float _NoiseStrength;
            float _NoiseSpeed;
            float _NoiseSize;
            float _MovementSmoothing;

            // Pixel Displacement
            sampler2D _Row1Tex;
            float _RowSize;
            float _Offset;
            float _RowMoveSpeed;
            float _DisplacementMovementSmoothing;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex.y += _ChromaticAmount + _Offset;
                o.vertex.z -= 0.02;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Main Texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;

                // Noise
                float2 screenUV = i.screenPos.xy / i.screenPos.w * _NoiseSize;

                float2 noiseUv = screenUV;
                float movementTime = round(_Time.y * _MovementSmoothing) / _MovementSmoothing;
                noiseUv = screenUV + float2(movementTime * _NoiseSpeed, movementTime * _NoiseSpeed);
                fixed4 noiseCol = tex2D(_NoiseMap, noiseUv);
                
                col.g = col.g + noiseCol.g * _NoiseStrength * 5;
                col.g = clamp(col.g, 0, 1);

                // Chromatic Aberration Green Color
                col.r = 0;
                col.b = 0;
                col.a = col.g * _TransparencyAmount;

                // Pixel Displacement
                float3 objectPosition = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                float3 cameraPosition = _WorldSpaceCameraPos;
                float distanceToCamera = distance(objectPosition, cameraPosition);
                float adjustedRowSize = _RowSize * distanceToCamera;

                float displacementMovementTime = round(_Time.y * _DisplacementMovementSmoothing) / _DisplacementMovementSmoothing;
                screenUV = i.screenPos.xy / i.screenPos.w * adjustedRowSize + displacementMovementTime * _RowMoveSpeed;

                fixed4 rowOverlay = tex2D(_Row1Tex, screenUV);

                col.a *= rowOverlay.g;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        // Green_B
        Pass
        {
            Name "Green_Channel_B"
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
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
            float4 _Color;

            // Chromatic Aberration
            float4 _MainTex_ST;
            float _ChromaticAmount;
            float _TransparencyAmount;

            // Noise
            sampler2D _NoiseMap;
            float _NoiseStrength;
            float _NoiseSpeed;
            float _NoiseSize;
            float _MovementSmoothing;

            // Pixel Displacement
            sampler2D _Row1Tex;
            float _RowSize;
            float _Offset;
            float _RowMoveSpeed;
            float _DisplacementMovementSmoothing;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex.y += _ChromaticAmount - _Offset;
                o.vertex.z -= 0.02;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Main Texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;

                // Noise
                float2 screenUV = i.screenPos.xy / i.screenPos.w * _NoiseSize;

                float2 noiseUv = screenUV;
                float movementTime = round(_Time.y * _MovementSmoothing) / _MovementSmoothing;
                noiseUv = screenUV + float2(movementTime * _NoiseSpeed, movementTime * _NoiseSpeed);
                fixed4 noiseCol = tex2D(_NoiseMap, noiseUv);
                
                col.g = col.g + noiseCol.g * _NoiseStrength * 5;
                col.g = clamp(col.g, 0, 1);

                // Chromatic Aberration Green Color
                col.r = 0;
                col.b = 0;
                col.a = col.g * _TransparencyAmount;

                // Pixel Displacement
                float3 objectPosition = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                float3 cameraPosition = _WorldSpaceCameraPos;
                float distanceToCamera = distance(objectPosition, cameraPosition);
                float adjustedRowSize = _RowSize * distanceToCamera;

                float displacementMovementTime = round(_Time.y * _DisplacementMovementSmoothing) / _DisplacementMovementSmoothing;
                screenUV = i.screenPos.xy / i.screenPos.w * adjustedRowSize + displacementMovementTime * _RowMoveSpeed;

                fixed4 rowOverlay = tex2D(_Row1Tex, screenUV);

                rowOverlay = 1 - rowOverlay;

                col.a *= rowOverlay.g;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        // Blue_W
        Pass
        {
            Name "Blue_Channel_W"
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
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
            float4 _Color;

            // Chromatic Aberration
            float4 _MainTex_ST;
            float _ChromaticAmount;
            float _TransparencyAmount;

            // Noise
            sampler2D _NoiseMap;
            float _NoiseStrength;
            float _NoiseSpeed;
            float _NoiseSize;
            float _MovementSmoothing;

            // Pixel Displacement
            sampler2D _Row1Tex;
            float _RowSize;
            float _Offset;
            float _RowMoveSpeed;
            float _DisplacementMovementSmoothing;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex.x -= _ChromaticAmount + _Offset;
                o.vertex.z -= 0.02;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Main Texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;

                // Noise
                float2 screenUV = i.screenPos.xy / i.screenPos.w * _NoiseSize;

                float2 noiseUv = screenUV;
                float movementTime = round(_Time.y * _MovementSmoothing) / _MovementSmoothing;
                noiseUv = screenUV + float2(movementTime * _NoiseSpeed, movementTime * _NoiseSpeed);
                fixed4 noiseCol = tex2D(_NoiseMap, noiseUv);
                
                col.b = col.b + noiseCol.b * _NoiseStrength * 5;
                col.b = clamp(col.b, 0, 1);

                // Chromatic Aberration Green Color
                col.r = 0;
                col.g = 0;
                col.a = col.b * _TransparencyAmount;

                // Pixel Displacement
                float3 objectPosition = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                float3 cameraPosition = _WorldSpaceCameraPos;
                float distanceToCamera = distance(objectPosition, cameraPosition);
                float adjustedRowSize = _RowSize * distanceToCamera;

                float displacementMovementTime = round(_Time.y * _DisplacementMovementSmoothing) / _DisplacementMovementSmoothing;
                screenUV = i.screenPos.xy / i.screenPos.w * adjustedRowSize + displacementMovementTime * _RowMoveSpeed;

                fixed4 rowOverlay = tex2D(_Row1Tex, screenUV);

                col.a *= rowOverlay.b;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
        
        // Blue_B
        Pass
        {
            Name "Blue_Channel_B"
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
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
            float4 _Color;

            // Chromatic Aberration
            float4 _MainTex_ST;
            float _ChromaticAmount;
            float _TransparencyAmount;

            // Noise
            sampler2D _NoiseMap;
            float _NoiseStrength;
            float _NoiseSpeed;
            float _NoiseSize;
            float _MovementSmoothing;

            // Pixel Displacement
            sampler2D _Row1Tex;
            float _RowSize;
            float _Offset;
            float _RowMoveSpeed;
            float _DisplacementMovementSmoothing;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex.x -= _ChromaticAmount - _Offset;
                o.vertex.z -= 0.02;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Main Texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;

                // Noise
                float2 screenUV = i.screenPos.xy / i.screenPos.w * _NoiseSize;

                float2 noiseUv = screenUV;
                float movementTime = round(_Time.y * _MovementSmoothing) / _MovementSmoothing;
                noiseUv = screenUV + float2(movementTime * _NoiseSpeed, movementTime * _NoiseSpeed);
                fixed4 noiseCol = tex2D(_NoiseMap, noiseUv);
                
                col.b = col.b + noiseCol.b * _NoiseStrength * 5;
                col.b = clamp(col.b, 0, 1);

                // Chromatic Aberration Green Color
                col.r = 0;
                col.g = 0;
                col.a = col.b * _TransparencyAmount;

                // Pixel Displacement
                float3 objectPosition = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                float3 cameraPosition = _WorldSpaceCameraPos;
                float distanceToCamera = distance(objectPosition, cameraPosition);
                float adjustedRowSize = _RowSize * distanceToCamera;

                float displacementMovementTime = round(_Time.y * _DisplacementMovementSmoothing) / _DisplacementMovementSmoothing;
                screenUV = i.screenPos.xy / i.screenPos.w * adjustedRowSize + displacementMovementTime * _RowMoveSpeed;

                fixed4 rowOverlay = tex2D(_Row1Tex, screenUV);

                rowOverlay = 1 - rowOverlay;

                col.a *= rowOverlay.b;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
