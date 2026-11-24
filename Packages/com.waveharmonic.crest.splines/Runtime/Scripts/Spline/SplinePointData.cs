// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Base class for components which hold point-level spline data.
    /// </summary>
    [@ExecuteDuringEditMode]
    public abstract class SplinePointData : ManagedBehaviour<WaterRenderer>
    {
        internal abstract Vector4 GetData(Vector4 data);

#if UNITY_EDITOR
        [@OnChange(skipIfInactive: false)]
        private protected virtual void OnChange(string path, object previous)
        {
            if (!isActiveAndEnabled) return;

            if (Transform.parent == null)
            {
                return;
            }

            Spline.NotifyReceivers(Transform.parent);
        }
#endif

        private protected override void Initialize()
        {
            base.Initialize();
            Spline.NotifyReceivers(Transform.parent);
        }

        private protected override void OnDisable()
        {
            base.OnDisable();
            Spline.NotifyReceivers(Transform.parent);
        }
    }
}
