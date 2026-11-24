// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Custom spline point data for scattering.
    /// </summary>
    [@HelpURL(typeof(ScatteringSplinePointData))]
    [@AddComponentMenu(Constants.k_MenuPrefixSpline + "Scattering Spline Point Data")]
    public sealed partial class ScatteringSplinePointData : SplinePointData
    {
        [Tooltip("Whether to override the scattering color instead of just the weight.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        bool _OverrideScattering;

        [Tooltip("The scattering color.")]
        [@Show(nameof(_OverrideScattering))]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        Color _Scattering = s_DefaultScattering;

        [Tooltip("The weight of the scattering color.")]
        [@Hide(nameof(_OverrideScattering))]
        [@Range(0, 1)]
        [@GenerateAPI]
        [SerializeField]
        float _Weight = 1f;

        internal static readonly Color s_DefaultScattering = ScatteringLod.s_DefaultColor;

        internal override Vector4 GetData(Vector4 data)
        {
            data.w = _Weight;
            return _OverrideScattering ? _Scattering.MaybeLinear() : data;
        }
    }
}
