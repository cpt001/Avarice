using UnityEngine;
using System.Collections.Generic;
using MapMagic.Core;
using MapMagic.Terrains;
using MapMagic.Products;
using Den.Tools;
using Den.Tools.Matrices;
using System.Reflection; // CRITICAL: Required for the collection lookup pass

public class MM2InterlinkAdaptor : MonoBehaviour
{
    [Header("Core Reference Fallbacks")]
    public MapMagicObject mapMagic;

    [Header("Arterial Road Sampling")]
    [Tooltip("The path search radius around this spawned village plaza.")]
    public float searchRadius = 60f;
    [Tooltip("Density of node sampling from the matrix layer (lower = denser points).")]
    [Range(2, 10)] public int nodeSampleStep = 4;

    public List<Vector3> capturedInterlinkNodes = new List<Vector3>();
    private bool hasExecuted = false;

    private void Start()
    {
        // When MapMagic instantiates an object, it places it inside the hierarchy of the corresponding chunk.
        // We can look upward to find the tile data directly without waiting for events!
        TryExecuteImmediateCapture();
    }

    private void TryExecuteImmediateCapture()
    {
        if (hasExecuted) return;

        if (mapMagic == null) mapMagic = FindFirstObjectByType<MapMagicObject>();

        // 1. CHUNK LOOKUP: Find the MapMagic TerrainTile component that owns this spawned object
        TerrainTile parentTile = GetComponentInParent<TerrainTile>();

        if (parentTile == null)
        {
            // Fallback: If not parented yet, look up via world coordinates
            if (mapMagic != null && mapMagic.tiles != null && mapMagic.tiles.grid != null)
            {
                float exactChunkX = transform.position.x / mapMagic.tileSize.x;
                float exactChunkZ = transform.position.z / mapMagic.tileSize.z;
                Coord townTileCoord = Coord.Floor(exactChunkX, exactChunkZ);
                mapMagic.tiles.grid.TryGetValue(townTileCoord, out parentTile);
            }
        }

        if (parentTile != null && mapMagic != null)
        {
            // 2. DATA EXTRACTION: Grab the live baked matrix cache directly from the active tile pin
            // ALTERNATIVE FIX: Directly extracts the data product arrays straight off the chunk node component itself
            TileData tileData = parentTile.main != null ? parentTile.main.data : null;


            if (tileData != null)
            {
                Debug.Log($"[MM2 Object Flow] Successfully intercepted TileData for chunk [{parentTile.coord.x},{parentTile.coord.z}] from inside the Object Flow!", this);
                hasExecuted = true;
                ProcessTileDataDirect(parentTile, tileData);
            }
            else
            {
                // If MapMagic hasn't fully written the cache to the grid yet, delay by one frame and retry
                Invoke(nameof(TryExecuteImmediateCapture), 0.1f);
            }
        }
        else
        {
            // Retry safety buffer if parenting sequence is still handling assignment threads
            Invoke(nameof(TryExecuteImmediateCapture), 0.1f);
        }
    }


    private void ProcessTileDataDirect(TerrainTile tile, TileData tileData)
    {
        capturedInterlinkNodes.Clear();

        float worldMinX = tile.coord.x * mapMagic.tileSize.x;
        float worldMinZ = tile.coord.z * mapMagic.tileSize.z;

        MatrixWorld pathMatrix = null;

        if (tileData.biomeMask != null)
        {
            pathMatrix = tileData.biomeMask;
        }
        else
        {
            // ========================================================
            // THE FIX: REFLECTION-BASED MATRIX RECOVERY
            // ========================================================
            // Dynamically find the underlying products collection hidden inside TileData
            FieldInfo productsField = typeof(TileData).GetField("products", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (productsField != null)
            {
                var hiddenProducts = productsField.GetValue(tileData) as System.Collections.IDictionary;

                if (hiddenProducts != null)
                {
                    foreach (System.Collections.DictionaryEntry entry in hiddenProducts)
                    {
                        // Look through the unexposed data payloads for a valid world matrix
                        if (entry.Value is MatrixWorld matrix && matrix != null)
                        {
                            // Ensure we ignore the basic heightmap layer
                            if (matrix != tileData.heights)
                            {
                                pathMatrix = matrix;
                                break;
                            }
                        }
                    }
                }
            }
            // ========================================================
        }

        // Ultimate fallback if no custom biome/graph matrices are uncovered
        if (pathMatrix == null)
        {
            pathMatrix = tileData.heights;
        }

        if (pathMatrix != null)
        {
            int minX = pathMatrix.rect.Min.x;
            int maxX = pathMatrix.rect.Max.x;
            int minZ = pathMatrix.rect.Min.z;
            int maxZ = pathMatrix.rect.Max.z;

            float cellWidth = mapMagic.tileSize.x / pathMatrix.rect.size.x;
            float cellLength = mapMagic.tileSize.z / pathMatrix.rect.size.z;

            for (int x = minX; x < maxX; x += nodeSampleStep)
            {
                for (int z = minZ; z < maxZ; z += nodeSampleStep)
                {
                    float pathWeight = pathMatrix[x, z];

                    if (pathWeight > 0.02f)
                    {
                        float relativeX = (x - minX) * cellWidth;
                        float relativeZ = (z - minZ) * cellLength;
                        Vector3 worldPos = new Vector3(worldMinX + relativeX, 0f, worldMinZ + relativeZ);

                        if (tileData.heights != null)
                        {
                            worldPos.y = tileData.heights[x, z] * mapMagic.globals.height;
                        }
                        else
                        {
                            worldPos.y = transform.position.y;
                        }

                        if (Vector3.Distance(transform.position, worldPos) <= searchRadius)
                        {
                            capturedInterlinkNodes.Add(worldPos);
                        }
                    }
                }
            }
        }

        if (capturedInterlinkNodes.Count > 0)
        {
            Debug.Log($"[MM2 Object Flow] Success! Reflection pass uncoupled {capturedInterlinkNodes.Count} real-world nodes within the town footprint.");
            GenerateVisualRoadMarkers();
        }
        else
        {
            Debug.LogWarning("[MM2 Object Flow] Matrix parsing complete, but zero nodes met radius or threshold limits.");
        }
    }



private void GenerateVisualRoadMarkers()
    {
        foreach (Vector3 node in capturedInterlinkNodes)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = $"Visual_RoadMatrixNode_{Mathf.RoundToInt(node.x)}_{Mathf.RoundToInt(node.z)}";

            marker.transform.position = node;
            marker.transform.localScale = new Vector3(1.2f, 4f, 1.2f);
            marker.transform.SetParent(this.transform);

            if (marker.TryGetComponent(out Collider c)) Destroy(c);

            if (marker.TryGetComponent(out Renderer rend))
            {
                rend.material = new Material(Shader.Find("Sprites/Default"));
                rend.material.color = Color.magenta;
            }
        }
    }
}
