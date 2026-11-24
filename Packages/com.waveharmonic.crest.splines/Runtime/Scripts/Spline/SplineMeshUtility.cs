// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Generates mesh suitable for rendering Gerstner waves from a spline
    /// </summary>
    static class SplineMeshUtility
    {
        static readonly List<SplinePoint> s_SplinePoints = new();

        public static bool GenerateMeshFromSpline<T>(Spline spline, Transform transform, int subdivisions, float radius, Vector4 customDataDefault, ref Mesh mesh, ref Vector3[] verts, bool combined = false)
            where T : SplinePointData
        {
            spline.GetComponentsInChildren(includeInactive: false, s_SplinePoints);

            if (s_SplinePoints.Count < 2)
            {
                s_SplinePoints.Clear();
                mesh = null;
                return false;
            }

            var splinePointCount = s_SplinePoints.Count;
            if (spline._Closed && splinePointCount > 2)
            {
                splinePointCount++;
            }

            var poolV3 = ArrayPool<Vector3>.Shared;
            var poolV4 = ArrayPool<Vector4>.Shared;

            var pointsCount = (splinePointCount - 1) * 3 + 1;
            var pointsArray = poolV3.Rent(pointsCount);
            var points = pointsArray.AsSpan()[..pointsCount];

            if (!SplineInterpolation.GenerateCubicSplineHull(s_SplinePoints, points, spline._Closed))
            {
                s_SplinePoints.Clear();
                return false;
            }

            // SplinePointData (base class) means no custom data for this input.
            var hasDataSetup = typeof(T) != typeof(SplinePointData);

            // Sample spline

            // Estimate total length of spline and use this to compute a sample count
            var lengthEst = 0f;
            for (var i = 1; i < splinePointCount; i++)
            {
                lengthEst += (s_SplinePoints[i % s_SplinePoints.Count].Transform.position - s_SplinePoints[i - 1].Transform.position).magnitude;
            }
            lengthEst = Mathf.Max(lengthEst, 1f);

            var spacing = 16f / Mathf.Pow(2f, subdivisions + 1);
            var pointCount = Mathf.CeilToInt(lengthEst / spacing);
            pointCount = Mathf.Max(pointCount, 1);

            var sampledPtsOnSplineArray = poolV3.Rent(pointCount);
            var sampledPtsOffSplineLeftArray = poolV3.Rent(pointCount);
            var sampledPtsOffSplineRightArray = poolV3.Rent(pointCount);
            var sampledPtsOnSpline = sampledPtsOnSplineArray.AsSpan()[..pointCount];
            var sampledPtsOffSplineLeft = sampledPtsOffSplineLeftArray.AsSpan()[..pointCount];
            var sampledPtsOffSplineRight = sampledPtsOffSplineRightArray.AsSpan()[..pointCount];

            // First set of sample points lie on spline
            sampledPtsOnSpline[0] = points[0];

            // Custom spline data - specific to this construction
            var customDataArray = poolV4.Rent(pointCount);
            var customData = customDataArray.AsSpan()[..pointCount];

            Vector4[] customDataArray1 = null;
            Vector4[] customDataArray2 = null;
            Span<Vector4> customData1 = default;
            Span<Vector4> customData2 = default;

            var hasFlow = false;
            var hasFoam = false;
            var hasAbsorption = false;
            var hasScattering = false;

            var flowDefault = Vector4.zero;
            var foamDefault = Vector4.zero;
            var absorptionDefault = Vector4.zero;
            var scatteringDefault = Vector4.zero;

            if (combined)
            {
                hasFlow = spline._HasFlow;
                if (hasFlow) flowDefault = spline._FlowLodInput.GetData<SplineLodInputData>().DefaultCustomSplineData;
                hasAbsorption = spline._HasAbsorption;
                if (hasAbsorption) absorptionDefault = spline._AbsorptionLodInput.GetData<SplineLodInputData>().DefaultCustomSplineData;
                hasScattering = spline._HasScattering;
                if (hasScattering) scatteringDefault = spline._ScatteringLodInput.GetData<SplineLodInputData>().DefaultCustomSplineData;

                customDataArray1 = poolV4.Rent(pointCount);
                customDataArray2 = poolV4.Rent(pointCount);
                customData1 = customDataArray1.AsSpan()[..pointCount];
                customData2 = customDataArray2.AsSpan()[..pointCount];

                var cd0 = Vector4.zero;
                var cd1 = Vector4.zero;
                var cd2 = Vector4.zero;

                if (hasFlow)
                {
                    cd0.z = GetCustomData0<FlowSplinePointData>(flowDefault, hasFlow).x;
                }

                if (hasFoam)
                {
                    cd0.w = GetCustomData0<FoamSplinePointData>(foamDefault, hasFoam).x;
                }

                if (hasAbsorption)
                {
                    cd1 = GetCustomData0<AbsorptionSplinePointData>(absorptionDefault, hasAbsorption);
                }

                if (hasScattering)
                {
                    cd2 = GetCustomData0<ScatteringSplinePointData>(scatteringDefault, hasScattering);
                }

                customData[0] = cd0;
                customData1[0] = cd1;
                customData2[0] = cd2;
            }
            else
            {
                customData[0] = GetCustomData0<T>(customDataDefault, hasDataSetup);
            }

            for (var i = 1; i < pointCount; i++)
            {
                var t = i / (float)(pointCount - 1);
                SplineInterpolation.InterpolateCubicPosition(splinePointCount, points, t, out sampledPtsOnSpline[i]);
            }

            float radiusLeft, radiusRight;
            if (spline._Offset == SplineOffset.Left)
            {
                radiusLeft = radius * 2f;
                radiusRight = 0f;
            }
            else if (spline._Offset == SplineOffset.Center)
            {
                radiusLeft = radiusRight = radius;
            }
            else
            {
                radiusLeft = 0f;
                radiusRight = radius * 2f;
            }

            // Compute pairs of points to form the ribbon
            for (var i = 0; i < pointCount; i++)
            {
                var ibefore = i - 1;
                var iafter = i + 1;
                if (!spline._Closed)
                {
                    // Not closed - clamp to range
                    ibefore = Mathf.Max(ibefore, 0);
                    iafter = Mathf.Min(iafter, pointCount - 1);
                }
                else
                {
                    // Closed - wrap into range
                    if (ibefore < 0) ibefore += pointCount;
                    iafter %= pointCount;
                }

                var radiusMultiplier = 0f;
                if (i > 0)
                {
                    var t = i / (float)(pointCount - 1);
                    var tpts = t * (s_SplinePoints.Count - 1f);
                    var spidx = Mathf.FloorToInt(tpts);
                    var alpha = tpts - spidx;

                    // Interpolate default data
                    var splineData0 = s_SplinePoints[spidx]._RadiusMultiplier;
                    var splineData1 = s_SplinePoints[Mathf.Min(spidx + 1, s_SplinePoints.Count - 1)]._RadiusMultiplier;
                    radiusMultiplier = Mathf.Lerp(splineData0, splineData1, Mathf.SmoothStep(0f, 1f, alpha));

                    if (combined)
                    {
                        var cd0 = Vector4.zero;
                        var cd1 = Vector4.zero;
                        var cd2 = Vector4.zero;

                        if (hasFlow)
                        {
                            cd0.z = GetCustomData<FlowSplinePointData>(flowDefault, hasFlow, spidx, alpha).x;
                        }

                        if (hasFoam)
                        {
                            cd0.w = GetCustomData<FoamSplinePointData>(foamDefault, hasFoam, spidx, alpha).x;
                        }

                        if (hasAbsorption)
                        {
                            cd1 = GetCustomData<AbsorptionSplinePointData>(absorptionDefault, hasAbsorption, spidx, alpha);
                        }

                        if (hasScattering)
                        {
                            cd2 = GetCustomData<ScatteringSplinePointData>(scatteringDefault, hasScattering, spidx, alpha);
                        }

                        customData[i] = cd0;
                        customData1[i] = cd1;
                        customData2[i] = cd2;
                    }
                    else
                    {
                        // Interpolate custom data
                        customData[i] = GetCustomData<T>(customDataDefault, hasDataSetup, spidx, alpha);
                    }
                }
                else
                {
                    radiusMultiplier = s_SplinePoints[0]._RadiusMultiplier;
                }

                var tangent = sampledPtsOnSpline[iafter] - sampledPtsOnSpline[ibefore];
                var normal = tangent;
                normal.x = tangent.z;
                normal.z = -tangent.x;
                normal.y = 0f;
                normal = normal.normalized;
                sampledPtsOffSplineLeft[i] = sampledPtsOnSpline[i] - radiusLeft * radiusMultiplier * normal;
                sampledPtsOffSplineRight[i] = sampledPtsOnSpline[i] + radiusMultiplier * radiusRight * normal;
            }

            if (spline._Closed)
            {
                var midPoint = Vector3.Lerp(sampledPtsOffSplineRight[0], sampledPtsOffSplineRight[^1], 0.5f);
                sampledPtsOffSplineRight[0] = sampledPtsOffSplineRight[^1] = midPoint;
            }

            // Fix cases where points reverse direction causing flipped triangles in result
            ResolveOverlaps(sampledPtsOffSplineLeft, sampledPtsOnSpline);
            ResolveOverlaps(sampledPtsOffSplineRight, sampledPtsOnSpline);

            // Do a few smoothing iterations just to try to soften results
            // Reuse sampledPtsOnSpline, as no longer needed.
            for (var j = 0; j < 5; j++)
            {
                for (var i = 1; i < sampledPtsOffSplineLeft.Length - 1; i++)
                {
                    sampledPtsOnSpline[i] = 0.5f * (sampledPtsOffSplineLeft[i - 1] + sampledPtsOffSplineLeft[i + 1]);
                }
                for (var i = 1; i < sampledPtsOffSplineLeft.Length - 1; i++)
                {
                    sampledPtsOffSplineLeft[i] = sampledPtsOnSpline[i];
                }

                for (var i = 1; i < sampledPtsOffSplineRight.Length - 1; i++)
                {
                    sampledPtsOnSpline[i] = 0.5f * (sampledPtsOffSplineRight[i - 1] + sampledPtsOffSplineRight[i + 1]);
                }
                for (var i = 1; i < sampledPtsOffSplineRight.Length - 1; i++)
                {
                    sampledPtsOffSplineRight[i] = sampledPtsOnSpline[i];
                }
            }

            // Update spline mesh.
            // Generates a mesh from the points sampled along the spline, and corresponding
            // offset points. Bridges points with a ribbon of triangles.
            {
                if (mesh == null)
                {
                    mesh = new();
                    mesh.name = $"{transform.gameObject.name}_SplineMesh";
                }
                else
                {
                    // Make sure to clear existing indices etc or incur errors.
                    mesh.Clear();
                }

                // This shows the setup if spline offset is 'right' - ribbon extends out to right hand side of spline
                //                       \
                //               \   ___--4
                //                4--      \
                //                 \        \
                //  splinePoint1 -> 3--------3
                //                  |        |
                //                  2--------2
                //                  |        |
                //                  1--------1
                //                  |        |
                //  splinePoint0 -> 0--------0
                //
                //                  ^        ^
                // sampledPointsOnSpline   sampledPointsOffSpline
                //

                var triCount = (sampledPtsOffSplineLeft.Length - 1) * 2;
                var indicesCount = triCount * 3;
                var poolI1 = ArrayPool<int>.Shared;
                var indices = poolI1.Rent(indicesCount);
                var vertCount = 2 * sampledPtsOffSplineLeft.Length;
                if (vertCount != verts?.Length)
                {
                    // Consumer will use vertices to calculate a transformed bounds like a renderer would.
                    verts = new Vector3[vertCount];
                }

                var uvs0 = poolV4.Rent(vertCount);
                var uvs1 = poolV4.Rent(vertCount);
                Vector4[] uvs2 = null;
                Vector4[] uvs3 = null;

                if (combined)
                {
                    uvs2 = poolV4.Rent(vertCount);
                    uvs3 = poolV4.Rent(vertCount);
                }

                transform.InverseTransformPoints(sampledPtsOffSplineLeft, sampledPtsOffSplineLeft);
                transform.InverseTransformPoints(sampledPtsOffSplineRight, sampledPtsOffSplineRight);

                // This iterates over result points and emits a quad starting from the current result points (resultPts0[i0], resultPts1[i1]) to
                // the next result points. If the spline is closed, last quad bridges the last result points and the first result points.
                for (var i = 0; i < sampledPtsOffSplineLeft.Length; i += 1)
                {
                    // Vert indices:
                    //
                    //              2i1------2i1+1
                    //               |\       |
                    //               |  \     |
                    //               |    \   |
                    //               |      \ |
                    //              2i0------2i0+1
                    //               |        |
                    //               ~        ~
                    //               |        |
                    //    splinePoint0--------|
                    //

                    verts[2 * i] = sampledPtsOffSplineLeft[i];
                    verts[2 * i + 1] = sampledPtsOffSplineRight[i];

                    var axis0 = new Vector2(verts[2 * i].x - verts[2 * i + 1].x, verts[2 * i].z - verts[2 * i + 1].z).normalized;

                    // x - 1-0 formerly known as inverted normalized distance from shoreline.
                    // y use 0.5 to exclude from feathering.
                    uvs0[2 * i] = new Vector4(1f, 0.5f, axis0.x, axis0.y);
                    uvs0[2 * i + 1] = new Vector4(0f, 0.5f, axis0.x, axis0.y);

                    uvs1[2 * i] = customData[i];
                    uvs1[2 * i + 1] = customData[i];

                    if (combined)
                    {
                        uvs2[2 * i] = customData1[i];
                        uvs2[2 * i + 1] = customData1[i];
                        uvs3[2 * i] = customData2[i];
                        uvs3[2 * i + 1] = customData2[i];
                    }

                    // Emit two triangles
                    if (i < sampledPtsOffSplineLeft.Length - 1)
                    {
                        var inext = i + 1;

                        indices[i * 6] = 2 * i;
                        indices[i * 6 + 1] = 2 * inext;
                        indices[i * 6 + 2] = 2 * i + 1;

                        indices[i * 6 + 3] = 2 * inext;
                        indices[i * 6 + 4] = 2 * inext + 1;
                        indices[i * 6 + 5] = 2 * i + 1;
                    }
                }

                mesh.indexFormat = indicesCount > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;

                mesh.SetVertices(verts);
                mesh.SetUVs(0, uvs0, 0, vertCount);
                mesh.SetUVs(1, uvs1, 0, vertCount);
                if (combined)
                {
                    mesh.SetUVs(2, uvs2, 0, vertCount);
                    mesh.SetUVs(3, uvs3, 0, vertCount);
                }

                mesh.SetIndices(indices, 0, indicesCount, MeshTopology.Triangles, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();

                poolI1.Return(indices);
                poolV4.Return(uvs0);
                poolV4.Return(uvs1);

                if (combined)
                {
                    poolV4.Return(uvs2);
                    poolV4.Return(uvs3);
                }
            }

            poolV3.Return(pointsArray);
            poolV3.Return(sampledPtsOnSplineArray);
            poolV3.Return(sampledPtsOffSplineLeftArray);
            poolV3.Return(sampledPtsOffSplineRightArray);
            poolV4.Return(customDataArray);

            if (combined)
            {
                poolV4.Return(customDataArray1);
                poolV4.Return(customDataArray2);
            }

            s_SplinePoints.Clear();

            return true;
        }

        static Vector4 GetCustomData<T>(Vector4 customDataDefault, bool hasDataSetup, int spidx, float alpha) where T : SplinePointData
        {
            var customData00 = customDataDefault;
            if (hasDataSetup && s_SplinePoints[spidx].TryGetComponent(out T customDataComp0) && customDataComp0.isActiveAndEnabled)
            {
                customData00 = customDataComp0.GetData(customDataDefault);
            }
            var customData01 = customDataDefault;
            if (hasDataSetup && s_SplinePoints[Mathf.Min(spidx + 1, s_SplinePoints.Count - 1)].TryGetComponent(out T customDataComp1) && customDataComp1.isActiveAndEnabled)
            {
                customData01 = customDataComp1.GetData(customDataDefault);
            }
            return Vector4.Lerp(customData00, customData01, Mathf.SmoothStep(0f, 1f, alpha));
        }

        static Vector4 GetCustomData0<T>(Vector4 customDataDefault, bool hasDataSetup) where T : SplinePointData
        {
            if (hasDataSetup && s_SplinePoints[0].TryGetComponent(out T customDataComp00) && customDataComp00.isActiveAndEnabled)
            {
                return customDataComp00.GetData(customDataDefault);
            }

            return customDataDefault;
        }

        // Ensures that the set of points are always moving "forwards", where forwards direction is defined by
        // the spline points
        static void ResolveOverlaps(Span<Vector3> points, Span<Vector3> pointsOnSpline)
        {
            if (points.Length < 2)
            {
                return;
            }

            // For each point after the first, check that it is "in front" of the last, compared
            // to the spline tangent
            var lastGoodPoint = points[1];
            for (var i = 1; i < points.Length; i++)
            {
                var point = points[i];
                var tangentSpline = pointsOnSpline[i] - pointsOnSpline[i - 1];
                var tangent = point - lastGoodPoint;

                // Do things flatland, weird cases can arise in full 3D
                tangent.y = tangentSpline.y = 0f;

                // Check if point has moved forward or not
                var dp = Vector3.Dot(tangent, tangentSpline);

                if (dp > 0f)
                {
                    // Forward movement, all good
                    lastGoodPoint = point;
                }
                else
                {
                    // Backpedal - use last good forward-moving point
                    // But keep y value, to help avoid a bunch of invalid points collapsing to a single point
                    points[i] = lastGoodPoint.XNZ(point.y);
                }
            }
        }
    }
}
