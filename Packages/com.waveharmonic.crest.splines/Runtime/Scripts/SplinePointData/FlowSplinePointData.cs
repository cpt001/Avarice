// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Custom spline point data for flow
    /// </summary>
    [@HelpURL(typeof(FlowSplinePointData))]
    [@AddComponentMenu(Constants.k_MenuPrefixSpline + "Flow Spline Point Data")]
    public sealed partial class FlowSplinePointData : SplinePointData
    {
        internal const float k_DefaultSpeed = 2f;

        [Tooltip("Flow velocity (speed of flow in direction of spline).\n\nCan be negative to flip direction.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        float _FlowVelocity = k_DefaultSpeed;

        internal override Vector4 GetData(Vector4 _)
        {
            return new(_FlowVelocity, 0f, 0f, 0f);
        }
    }
}
