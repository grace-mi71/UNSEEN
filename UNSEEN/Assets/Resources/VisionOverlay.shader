Shader "UNSEEN/VisionOverlay"
{
    Properties
    {
        _Mode ("Mode", Float) = 0
        _TunnelRadius ("Tunnel Radius", Range(0.05, 1)) = 0.13
        _TunnelFeather ("Tunnel Feather", Range(0.01, 0.5)) = 0.07
        _CataractHaze ("Cataract Haze", Range(0, 1)) = 0.52
        _DarknessOpacity ("Darkness Opacity", Range(0, 1)) = 0.68
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZTest Always
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _Mode;
            float _TunnelRadius;
            float _TunnelFeather;
            float _CataractHaze;
            float _DarknessOpacity;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Mode 1: cataract haze, Mode 2: tunnel vision, Mode 3: darkness.
                if (_Mode < 0.5)
                    return half4(0, 0, 0, 0);

                if (_Mode < 1.5)
                {
                    float grain = frac(sin(dot(input.uv * 900.0, float2(12.9898, 78.233))) * 43758.5453);
                    float haze = saturate(_CataractHaze + (grain - 0.5) * 0.035);
                    return half4(0.94, 0.97, 0.92, haze);
                }

                if (_Mode < 2.5)
                {
                    float2 centered = input.uv - 0.5;
                    centered.x *= 1.15;
                    float distanceFromCenter = length(centered);
                    float alpha = smoothstep(_TunnelRadius, _TunnelRadius + _TunnelFeather, distanceFromCenter);
                    return half4(0, 0, 0, alpha);
                }

                return half4(0, 0, 0, _DarknessOpacity);
            }
            ENDHLSL
        }
    }
}
