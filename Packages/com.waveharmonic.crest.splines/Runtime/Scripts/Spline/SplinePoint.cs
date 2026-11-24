// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Spline point, intended to be child of Spline object
    /// </summary>
    [@ExecuteDuringEditMode]
    [@HelpURL(typeof(SplinePoint))]
    [AddComponentMenu(Constants.k_MenuPrefixSpline + "Spline Point")]
    public sealed partial class SplinePoint : CustomBehaviour
    {
        [Tooltip("Multiplier for spline radius.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        internal float _RadiusMultiplier = 1f;

        internal Vector3 _LocalPosition;

#if UNITY_EDITOR
        private protected override void OnStart()
        {
            base.OnStart();

            if (Transform.parent.TryGetComponent<Spline>(out var spline) && spline.HasSource)
            {
                // We rely on hasChanged event to update the spline.
                _LocalPosition = Transform.localPosition;
            }
        }

        void Update()
        {
            if (!Transform.hasChanged)
            {
                return;
            }

            if (_LocalPosition == Transform.localPosition)
            {
                return;
            }

            if (Transform.parent.TryGetComponent<Spline>(out var spline) && spline.HasSource)
            {
                // Do not allow transform changed with source.
                Transform.localPosition = _LocalPosition;
                Transform.hasChanged = false;
                return;
            }

            // Do not set this during initialization as it will prevent initial update (like
            // when adding a new spline point).
            _LocalPosition = Transform.localPosition;

            NotifyOnChange();
        }

        void LateUpdate()
        {
            Transform.hasChanged = false;
        }

        void NotifyOnChange()
        {
            if (Transform.parent == null)
            {
                return;
            }

            Spline.NotifyReceivers(Transform.parent);
        }

        private protected override void OnDisable()
        {
            base.OnDisable();

            NotifyOnChange();
        }

        [@OnChange]
        void OnChange(string path, object previous)
        {
            if (!isActiveAndEnabled) return;
            NotifyOnChange();
        }
#endif
    }

    interface IReceiveSplineChangeMessages
    {
        void OnSplineChange();
    }
}
