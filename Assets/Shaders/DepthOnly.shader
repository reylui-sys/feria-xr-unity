Shader "ARTEC_IM/DepthOnly"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }

        // =========================
        // PASS 1 → OCLUSSION (DEPTH ONLY)
        // =========================
        Pass
        {
            Name "DepthOnly"

            ZWrite On
            ColorMask 0

            // Stencil buffer used to not render pixels marked as holes with other shaders

            Stencil
            {
                Ref 1
                Comp NotEqual
                Pass Keep
            }
        }

        // =========================
        // PASS 2 → SHADOW RECEIVER
        // =========================
        Pass
        {
            Name "ShadowPass"
            Tags { "LightMode"="UniversalForward" }

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            // Stencil buffer used to not render pixels marked as holes with other shaders

            Stencil
            {
                Ref 1
                Comp NotEqual
                Pass Keep
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v)
            {
                Varyings o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(o.positionWS);

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                float shadow = MainLightRealtimeShadow(shadowCoord);

                float alpha = 1 - shadow;

                // suavizado
                alpha = smoothstep(0.2, 1.0, alpha);

                return half4(0, 0, 0, alpha * 0.5);
            }

            ENDHLSL
        }
    }
}