// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

#include "Packages/com.waveharmonic.crest.splines/Runtime/Shaders/Library/Macros.hlsl"

m_SplinesNameSpace

struct Parameters
{
    float2 _UV;
    float2 _Axis;
};

// It was reported that "Unpack" and "unpacked" are reserved words in PSSL.
// Also prefixing "packed" to be safe.
Parameters _Unpack(float4 _packed)
{
    Parameters _unpacked;
    _unpacked._UV = _packed.xy;
    _unpacked._Axis = _packed.zw;
    return _unpacked;
}

m_SplinesNameSpaceEnd
