// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

Shader "Hidden/Crest/Inputs/Foam/Spline Geometry"
{
    SubShader
    {
        Blend [_Crest_BlendSource] [_Crest_BlendTarget]
        BlendOp [_Crest_BlendOperation]
        Cull Back
        ZTest Always
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "UnityCG.cginc"
            #include "Packages/com.waveharmonic.crest.splines/Runtime/Shaders/Library/Common.hlsl"

            CBUFFER_START(CrestPerWaterInput)
            float _Crest_Weight;
            float _Crest_FeatherWidth;
            float _Crest_SimDeltaTime;
            CBUFFER_END

            struct Attributes
            {
                float3 _PositionOS : POSITION;
                float4 _Parameters : TEXCOORD0;
                float _Weight : TEXCOORD1;
            };

            struct Varyings
            {
                float4 _PositionCS : SV_POSITION;
                float _CoordinatesU : TEXCOORD0;
                float _Weight : TEXCOORD1;
            };

            Varyings Vertex(const Attributes i_Input)
            {
                Varyings output;
                output._PositionCS = UnityObjectToClipPos(float4(i_Input._PositionOS, 1.0));
                m_Crest::Splines::Parameters _unpacked = m_Crest::Splines::_Unpack(i_Input._Parameters);
                output._CoordinatesU = _unpacked._UV.x;
                output._Weight = i_Input._Weight;
                return output;
            }

            half4 Fragment(const Varyings i_Input) : SV_Target
            {
                float weight = i_Input._Weight;
                float distance = i_Input._CoordinatesU;

                // Feather at front/back.
                if (distance > 0.5)
                {
                    distance = 1.0 - distance;
                }

                weight *= min(distance / _Crest_FeatherWidth, 1.0);
                weight *= _Crest_Weight;

                return 0.25 * weight * _Crest_SimDeltaTime;
            }
            ENDCG
        }
    }
}
