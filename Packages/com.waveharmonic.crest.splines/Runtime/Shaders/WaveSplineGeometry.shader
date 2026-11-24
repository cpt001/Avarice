// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

// Generates waves from geometry that is rendered into the water simulation from a top down camera. Expects
// following data on verts:
//   - POSITION: Vert positions as normal.
//   - TEXCOORD0: Axis - direction for waves to travel. "Forward vector" for waves.
//   - TEXCOORD1: X - 0 at start of waves, 1 at end of waves
//
//  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ uv1.x = 0             |
//  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  |                    |  uv0 - wave direction vector
//  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  |                   \|/
//  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ uv1.x = 1
//  ------------------- shoreline --------------------
//

Shader "Hidden/Crest/Inputs/Shape Waves/Spline"
{
    CGINCLUDE
    #pragma vertex Vertex
    #pragma fragment Fragment
    // #pragma enable_d3d11_debug_symbols

    #pragma multi_compile_fragment _ d_Bicubic

    #define d_Quantize 1

    #include "UnityCG.cginc"

    #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Macros.hlsl"
    #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Globals.hlsl"
    #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/InputsDriven.hlsl"
    #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Cascade.hlsl"
    #include "Packages/com.waveharmonic.crest.splines/Runtime/Shaders/Library/Common.hlsl"
    #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Waves.hlsl"

    CBUFFER_START(CrestPerWaterInput)
    half _Crest_FeatherWaveStart;
    float _Crest_RespectShallowWaterAttenuation;
    int _Crest_WaveBufferSliceIndex;
    float _Crest_AverageWavelength;
    float _Crest_AttenuationInShallows;
    float _Crest_Weight;
    float2 _Crest_AxisX;
    half _Crest_MaximumAttenuationDepth;
    half _Crest_FeatherWidth;
    uint _Crest_Resolution;
    CBUFFER_END

    m_CrestNameSpace

    struct Attributes
    {
        float4 vertex : POSITION;
        float4 parameters : TEXCOORD0;
        float weight : TEXCOORD1;
    };

    struct Varyings
    {
        float4 vertex : SV_POSITION;
        float3 uv_slice : TEXCOORD1;
        float2 axis : TEXCOORD2;
        float3 worldPosScaled : TEXCOORD3;
        float2 invNormDistToShoreline_weight : TEXCOORD4;
        float2 worldPosXZ : TEXCOORD5;
    };

    Varyings Vertex(Attributes v)
    {
        Varyings o;

        const float3 positionOS = v.vertex.xyz;
        o.vertex = UnityObjectToClipPos(positionOS);
        const float3 worldPos = mul( unity_ObjectToWorld, float4(positionOS, 1.0) ).xyz;

        m_Crest::Splines::Parameters _unpacked = m_Crest::Splines::_Unpack(v.parameters);

        // UV coordinate into the cascade we are rendering into
        o.uv_slice = Cascade::MakeAnimatedWaves(_Crest_LodIndex).WorldToUV(worldPos.xz);

        o.worldPosXZ = worldPos.xz;

        // World pos prescaled by wave buffer size, suitable for using as UVs in fragment shader
        const float waveBufferSize = 0.5f * (1 << _Crest_WaveBufferSliceIndex);
        o.worldPosScaled = worldPos / waveBufferSize;

        o.invNormDistToShoreline_weight.x = _unpacked._UV.x;
        o.invNormDistToShoreline_weight.y = v.weight * _Crest_Weight;

        // Rotate forward axis around y-axis into world space
        o.axis = dot( _unpacked._Axis, _Crest_AxisX ) * unity_ObjectToWorld._m00_m20 + dot( _unpacked._Axis, float2(-_Crest_AxisX.y, _Crest_AxisX.x) ) * unity_ObjectToWorld._m02_m22;

        return o;
    }

    float4 Fragment(Varyings input)
    {
        float wt = input.invNormDistToShoreline_weight.y;

        // Feature at away from shore.
        wt *= min( input.invNormDistToShoreline_weight.x / _Crest_FeatherWaveStart, 1.0 );

        // Feather at front/back
        if (input.invNormDistToShoreline_weight.x > 0.5)
        {
            input.invNormDistToShoreline_weight.x = 1.0 - input.invNormDistToShoreline_weight.x;
        }
        wt *= min( input.invNormDistToShoreline_weight.x / _Crest_FeatherWidth, 1.0 );

        float alpha = wt;

        // Attenuate if depth is less than half of the average wavelength
        const half depth = Cascade::MakeDepth(_Crest_LodIndex).SampleSignedDepthFromSeaLevel(input.worldPosXZ) +
            Cascade::MakeLevel(_Crest_LodIndex).SampleLevel(input.worldPosXZ);
        half depth_wt = saturate(2.0 * depth / _Crest_AverageWavelength);
        if (_Crest_MaximumAttenuationDepth < k_Crest_MaximumWaveAttenuationDepth)
        {
            depth_wt = lerp(depth_wt, 1.0, saturate(depth / _Crest_MaximumAttenuationDepth));
        }
        const float attenuationAmount = _Crest_AttenuationInShallows * _Crest_RespectShallowWaterAttenuation;
        wt *= attenuationAmount * depth_wt + (1.0 - attenuationAmount);

        const float3 displacement = SampleWaves
        (
            input.worldPosScaled.xz,
            input.axis,
            _Crest_WaveBufferSliceIndex,
            _Crest_Resolution
        );

        return float4(displacement * wt, alpha);
    }

    m_CrestNameSpaceEnd

    m_CrestVertex
    m_CrestFragment(float4)
    ENDCG

    SubShader
    {
        // Either additive or alpha blend for geometry waves.
        Cull Back
        ZTest Always
        ZWrite Off

        Pass
        {
            Blend [_Crest_BlendSource] [_Crest_BlendTarget]
            CGPROGRAM
            ENDCG
        }

        Pass
        {
            Blend One One
            CGPROGRAM
            ENDCG
        }
    }
}
