using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;
using MapMagic.Nodes.SplinesGenerators;
using WaveHarmonic.Crest.Splines;
using Spline = UnityEngine.Splines.Spline;
using System.Collections;
/// <summary>
/// Order ops:
/// 
/// Instantiate crest spline object, make sure the prefab to instantiate is done so inactive. 
/// Get origin (should match this object's transform?) - map: 527.7219, 147.6332, 650.5933 -> spline: 525.2976, 252.9782, 643.3532 - Doesn't. Result of relax node.
/// Clone spline container data, follow path, then remove unconnected nodes. 
/// Adjust y value foreach in container data to y=y-100
/// Apply new river spline to crest spline
/// Add flow, speed negative. Add level. 
/// </summary>
/// 
[ExecuteAlways]
public class CrestMM2RiverBuilder : MonoBehaviour
{
    [SerializeField] private GameObject riverPrefab;

    private void Start() => HandleSplinesCreated();
    private void OnEnable() => UnitySplinesOutput214.OnSplineCreated += HandleSplinesCreated;
    private void OnDisable() => UnitySplinesOutput214.OnSplineCreated -= HandleSplinesCreated;
    
    void HandleSplinesCreated()
    {
        if (!Application.isPlaying) return;
        if (riverPrefab == null) { Debug.LogWarning("RiverBuilder: no prefab assigned"); return; }

        Debug.Log("Spline created event");
        foreach (var src in FindObjectsByType<SplineContainer>(FindObjectsSortMode.None))
        {
            if (src.GetComponent<RiverProcessed>()) continue;
            src.gameObject.AddComponent<RiverProcessed>();

            for (int i = 0; i < src.Splines.Count; i++)
            {
                StartCoroutine(BuildRiver(src, src.Splines[i]));
            }
        }
    }

    [SerializeField] float pointSpacing = 8f;    // units between points
    [SerializeField] float waterDepth = 0.75f; // water surface above the riverbed
    [SerializeField] float waterfallDrop = 4f;    // drop between points that counts as a fall
    [SerializeField] LayerMask terrainMask;        // set to Main Terrain in the Inspector
    public List<Vector3> waterfalls = new List<Vector3>();

    private List<Vector3> ResamplePath(SplineContainer src, Spline s)
    {
        var pts = new List<Vector3>();
        int n = Mathf.Max(2, Mathf.CeilToInt(s.GetLength() / pointSpacing));
        for (int i = 0; i <= n; i++)
            pts.Add(src.transform.TransformPoint((Vector3)s.EvaluatePosition((float)i / n)));
        return pts;
    }

    private float[] SampleGround(List<Vector3> pts)
    {
        var g = new float[pts.Count];
        for (int i = 0; i < pts.Count; i++)
            g[i] = Physics.Raycast(pts[i] + Vector3.up * 1000f, Vector3.down, out var hit, 3000f, terrainMask)
                 ? hit.point.y
                 : pts[i].y - 100f;   // fallback if the ray misses: the old offset
                int misses = 0;
                // in the loop: if (!Physics.Raycast(...)) misses++;
                //Debug.Log($"River ground samples: {pts.Count}, misses: {misses}");
        return g;
    }

    private float[] FillFromMouth(float[] ground)
    {
        int n = ground.Length;
        var level = new float[n];
        level[n - 1] = ground[n - 1] + waterDepth;
        for (int i = n - 2; i >= 0; i--)
            level[i] = Mathf.Max(ground[i] + waterDepth, level[i + 1]);
        return level;
    }

    private IEnumerator BuildRiver(SplineContainer src, Spline srcSpline)
    {
        yield return new WaitForSeconds(0.5f);
        if (srcSpline.Count < 2) yield break;
        // 1. Evenly spaced points along MM2's path (world space)
        List<Vector3> pts = ResamplePath(src, srcSpline);

        // 2. Real ground height under each point
        float[] ground = SampleGround(pts);

        // 3. Flow downhill: the start should be the high end
        if (ground[0] < ground[ground.Length - 1])
        {
            pts.Reverse();
            System.Array.Reverse(ground);
        }

        // 4. Water levels: fill from the mouth upward (pools, no uphill flow)
        float[] level = FillFromMouth(ground);

        // 5. Build the Crest points
        GameObject go = Instantiate(riverPrefab, src.transform);   // inactive
        go.transform.localPosition = Vector3.zero;                  // world heights now, no -100

        for (int i = 0; i < pts.Count; i++)
        {
            var p = new GameObject("Spline Point");
            p.transform.SetParent(go.transform, false);
            p.transform.position = new Vector3(pts[i].x, level[i], pts[i].z);
            p.AddComponent<SplinePoint>();

            if (i < pts.Count - 1 && level[i] - level[i + 1] > waterfallDrop)
                waterfalls.Add(p.transform.position);
        }

        go.SetActive(true);
        yield return null;
    }
}
