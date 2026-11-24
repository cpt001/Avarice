// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WaveHarmonic.Crest.Splines.Editor
{
    static class Visualizers
    {
        static readonly List<SplinePoint> s_SplinePoints = new();

        static void SetLineColor(SplinePoint from, SplinePoint to, bool isClosing)
        {
            Gizmos.color = isClosing ? Color.white : Color.black * 0.5f;

            if (Selection.activeObject == from.gameObject || Selection.activeObject == to.gameObject)
            {
                Gizmos.color = Color.yellow;
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmos(Spline target, GizmoType type)
        {
            target.GetComponentsInChildren(includeInactive: false, s_SplinePoints);
            var points = s_SplinePoints;
            for (var i = 0; i < points.Count - 1; i++)
            {
                SetLineColor(points[i], points[i + 1], false);
                Gizmos.DrawLine(points[i].Transform.position, points[i + 1].Transform.position);
            }

            if (target._Closed && points.Count > 2)
            {
                SetLineColor(points[^1], points[0], true);
                Gizmos.DrawLine(points[^1].Transform.position, points[0].Transform.position);
            }

            Gizmos.color = Color.white;
            s_SplinePoints.Clear();
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawGizmos(LodInput target, GizmoType type)
        {
            if (target.Data is not SplineLodInputData data) return;
            var transform = target.Transform;
            Gizmos.color = target.GizmoColor;
            Gizmos.DrawWireMesh(data._Mesh, transform.position, transform.rotation, transform.lossyScale);
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        static void DrawGizmos(SplinePoint target, GizmoType type)
        {
            // We could not get gizmos or handles to work well when 3D Icons is enabled. problems included
            // them being almost invisible when occluded, or hard to select. DrawIcon is almost perfect,
            // but is very faint when occluded, but drawing it 8 times makes it easier to see.. sigh..
            var iconName = "d_Animation.Record@2x";
            Gizmos.DrawIcon(target.Transform.position, iconName, true);
            Gizmos.DrawIcon(target.Transform.position, iconName, true);
            Gizmos.DrawIcon(target.Transform.position, iconName, true);
            Gizmos.DrawIcon(target.Transform.position, iconName, true);
            Gizmos.DrawIcon(target.Transform.position, iconName, true);
            Gizmos.DrawIcon(target.Transform.position, iconName, true);
            Gizmos.DrawIcon(target.Transform.position, iconName, true);
            Gizmos.DrawIcon(target.Transform.position, iconName, true);

            if (type.HasFlag(GizmoType.Selected))
            {
                // Reduces spam. May have edge cases where spline will not update but that is fine for now.
                if (target.gameObject != Selection.activeGameObject)
                {
                    return;
                }

                foreach (var receiver in target.Transform.parent.GetComponents<LodInput>())
                {
                    DrawGizmos(receiver, type);
                }
            }
        }
    }
}
