using UnityEngine;
using System.Collections.Generic;
using MapMagic.Core;
using MapMagic.Terrains;
using MapMagic.Products;
using Den.Tools;
using Den.Tools.Matrices;
using MapMagic.Nodes;

public class MM2ModernInterlinkGrabber : MonoBehaviour
{
    [Header("Core References")]
    public MapMagicObject mapMagic;
    public Transform townCenterPlaza;
    private bool isInitialized;

    [Tooltip("The path search radius around the village plaza.")]
    public float searchRadius = 60f;

    [Tooltip("How dense or sparse you want your road sampling anchors to be.")]
    [Range(2, 10)]
    public int nodeSampleStep = 4;

    [HideInInspector]
    public List<Vector3> capturedInterlinkNodes = new List<Vector3>();

    private void OnEnable()
    {
        TerrainTile.OnTileApplied += HandleTileFinished;
    }

    private void OnDisable()
    {
        TerrainTile.OnTileApplied -= HandleTileFinished;
    }

    private void Start()
    {
        TryInitializeReferences();
    }

    private void Update()
    {
        if (!isInitialized)
        {
            TryInitializeReferences();
        }
    }

    private void TryInitializeReferences()
    {
        if (townCenterPlaza == null)
        {
            townCenterPlaza = this.transform;
        }
        if (mapMagic == null)
        {
            mapMagic = FindFirstObjectByType<MapMagicObject>();
        }
        if (townCenterPlaza != null && mapMagic != null)
        {
            isInitialized = true;
            //Debug.Log($"[Modern Interlink] References successfully linked! Town: {townCenterPlaza.name}, Engine: {mapMagic.name}", this);
        }
    }


    private void HandleTileFinished(TerrainTile tile, TileData tileData, StopToken stopToken)
    {
        TryInitializeReferences();
        if (tile == null || tileData == null || tileData.isDraft) return;
        if (townCenterPlaza == null || mapMagic == null) return;

        int chunkX = Mathf.FloorToInt(townCenterPlaza.position.x / mapMagic.tileSize.x);
        int chunkZ = Mathf.FloorToInt(townCenterPlaza.position.z / mapMagic.tileSize.z);
        if (tile.coord.x != chunkX || tile.coord.z != chunkZ) return;

        capturedInterlinkNodes.Clear();
        MatrixWorld pathMatrix = tileData.biomeMask;

        if (pathMatrix != null)
        {
            int minX = pathMatrix.rect.Min.x;
            int maxX = pathMatrix.rect.Max.x;
            int minZ = pathMatrix.rect.Min.z;
            int maxZ = pathMatrix.rect.Max.z;

            for (int x = minX; x < maxX; x += nodeSampleStep)
            {
                for (int z = minZ; z < maxZ; z += nodeSampleStep)
                {
                    float pathWeight = pathMatrix[x, z];

                    if (pathWeight > 0.2f)
                    {
                        float worldOriginX = tile.coord.x * mapMagic.tileSize.x;
                        float worldOriginZ = tile.coord.z * mapMagic.tileSize.z;

                        float cellWidth = mapMagic.tileSize.x / pathMatrix.rect.size.x;
                        float cellLength = mapMagic.tileSize.z / pathMatrix.rect.size.z;

                        float relativeX = (x - minX) * cellWidth;
                        float relativeZ = (z - minZ) * cellLength;
                        Vector3 worldPos = new Vector3(worldOriginX + relativeX, 0f, worldOriginZ + relativeZ);

                        if (tileData.heights != null)
                        {
                            worldPos.y = tileData.heights[x, z];
                        }
                        else if (Terrain.activeTerrain != null)
                        {
                            worldPos.y = Terrain.activeTerrain.SampleHeight(worldPos);
                        }
                        else
                        {
                            worldPos.y = 0f;
                        }
                    }
                }
            }
        }


        if (capturedInterlinkNodes.Count > 0)
        {
            Debug.Log($"[Modern Interlink] Captured {capturedInterlinkNodes.Count} real-world road coordinates!");
            //Voronoi handoff here
        }
    }

}
