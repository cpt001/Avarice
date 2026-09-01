using UnityEngine;
using System.Collections.Generic;
using MapMagic.Core;
using MapMagic.Terrains;
using MapMagic.Products;
using Den.Tools;
using Den.Tools.Matrices;

public class RuntimeVoronoiTownSetup : MonoBehaviour
{
    [Header("Core Reference Fallbacks")]
    public MapMagicObject mapMagic;

    [Header("Arterial Road Sampling")]
    [Tooltip("The path search radius around this spawned village plaza.")]
    public float townRadius = 60f;
    [Tooltip("Density of node sampling from the matrix layer (lower = denser points).")]
    [Range(2, 10)] public int nodeSampleStep = 4;

    // This array will permanently store the nodes once captured
    [HideInInspector]
    public List<Vector3> arterialRoadAnchors = new List<Vector3>();

    private void Awake()
    {
        if (mapMagic == null)
        {
            mapMagic = FindFirstObjectByType<MapMagicObject>();
        }

        if (mapMagic != null)
        {
            // 1. Calculate the exact MapMagic chunk coordinates for this instance
            float exactChunkX = transform.position.x / mapMagic.tileSize.x;
            float exactChunkZ = transform.position.z / mapMagic.tileSize.z;
            Coord townTileCoord = Coord.Floor(exactChunkX, exactChunkZ);

            // 2. Rename the clone so it stands out clearly in your hierarchy and console
            gameObject.name = $"TownSetup_Island_[{townTileCoord.x},{townTileCoord.z}]";

            Debug.Log($"[{gameObject.name}] Born at world position {transform.position}. Harvesting immediate spatial grid parameters...", this);

            // 3. TARGET FILTER: Only run the heavy matrix loops on your primary sample island chunk
            // Replace 0,0 with the coordinates of the specific island you want to test!
            if (townTileCoord.x == 0 && townTileCoord.z == 0)
            {
                ExecuteImmediateMatrixSweep();
            }
            else
            {
                Debug.Log($"[{gameObject.name}] Background island chunk detected. Stalling town graph generation until tile focuses.", this);
            }
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Runtime Town spawned, but no active MapMagicObject found in the scene!", this);
        }
    }


    private void ExecuteImmediateMatrixSweep()
    {
        if (mapMagic == null || mapMagic.tiles == null || mapMagic.tiles.grid == null) return;

        // Convert world position to MapMagic grid coordinates safely using floor truncation
        float exactChunkX = transform.position.x / mapMagic.tileSize.x;
        float exactChunkZ = transform.position.z / mapMagic.tileSize.z;
        Coord townTileCoord = Coord.Floor(exactChunkX, exactChunkZ);

        // Look up the active tile instance directly from MapMagic's operational grid
        if (mapMagic.tiles.grid.TryGetValue(townTileCoord, out TerrainTile liveTile))
        {
            ProcessTileDataDirect(liveTile);
        }
    }

    private void ProcessTileDataDirect(TerrainTile tile)
    {
        if (tile == null) return;

        arterialRoadAnchors.Clear();

        // 1. Calculate step sizes based on the terrain scale resolutions
        float stepSize = mapMagic.tileSize.x / 128f;

        // 2. Sweep a clean grid bounds square centered on this town's runtime spawn position
        for (float xOffset = -townRadius; xOffset <= townRadius; xOffset += nodeSampleStep * stepSize)
        {
            for (float zOffset = -townRadius; zOffset <= townRadius; zOffset += nodeSampleStep * stepSize)
            {
                Vector3 checkWorldPos = new Vector3(transform.position.x + xOffset, 0f, transform.position.z + zOffset);

                // FIX: Native Unity Terrain sampler seamlessly retrieves the true visual height
                if (Terrain.activeTerrain != null)
                {
                    checkWorldPos.y = Terrain.activeTerrain.SampleHeight(checkWorldPos);
                }
                else
                {
                    checkWorldPos.y = transform.position.y; // Fallback to town height if mesh loading is delayed
                }

                // 3. Filter down coordinates to only those sitting inside our city radius limits
                float distance = Vector3.Distance(transform.position, checkWorldPos);
                if (distance <= townRadius)
                {
                    arterialRoadAnchors.Add(checkWorldPos);
                }
            }
        }

        // 4. DIAGNOSTIC PRINT: This is guaranteed to fire on the exact frame the clone is born
        Debug.Log($"[{gameObject.name}] Spatial Grid Capture -> Seeded {arterialRoadAnchors.Count} structural nodes directly within town radius!", this);

        if (arterialRoadAnchors.Count > 0)
        {
            GenerateVoronoiArterialNetwork(arterialRoadAnchors);
        }
    }

    private void GenerateVoronoiArterialNetwork(List<Vector3> roadAnchors)
    {
        // Your custom street network generator (CsDelaunay / Delaunator) links here!
    }

    // Diagnostic Visualization tool paths
    private void OnDrawGizmosSelected()
    {
        if (arterialRoadAnchors == null || arterialRoadAnchors.Count == 0) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < arterialRoadAnchors.Count; i++)
        {
            Gizmos.DrawSphere(arterialRoadAnchors[i], 0.75f);
        }
        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Gizmos.DrawWireSphere(transform.position, townRadius);
    }
}
