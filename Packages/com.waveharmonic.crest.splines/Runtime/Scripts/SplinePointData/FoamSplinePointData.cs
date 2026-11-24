// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Foam adjustment parameter on spline points
    /// </summary>
    [@HelpURL(typeof(FoamSplinePointData))]
    [@AddComponentMenu(Constants.k_MenuPrefixSpline + "Foam Spline Point Data")]
    public sealed partial class FoamSplinePointData : SplinePointData
    {
        internal const float k_DefaultAmount = 1f;

        [Tooltip("Amount of foam emitted.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        float _FoamAmount = k_DefaultAmount;

        internal override Vector4 GetData(Vector4 _)
        {
            return new(_FoamAmount, 0f, 0f, 0f);
        }
    }
}
