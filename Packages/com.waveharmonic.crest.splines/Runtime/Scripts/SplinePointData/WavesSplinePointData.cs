// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Custom spline point data for waves
    /// </summary>
    [@HelpURL(typeof(WavesSplinePointData))]
    [@AddComponentMenu(Constants.k_MenuPrefixSpline + "Waves Spline Point Data")]
    public sealed partial class WavesSplinePointData : SplinePointData
    {
        internal const float k_DefaultWeight = 1f;

        [@Label("Wave Multiplier")]
        [Tooltip("Weight multiplier to scale waves.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        float _Weight = k_DefaultWeight;

        internal override Vector4 GetData(Vector4 _)
        {
            return new(_Weight, 0f, 0f, 0f);
        }
    }
}
