using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MapMagic.Core;
using MapMagic.Terrains;
using MapMagic.Nodes;
using Den.Tools;

/// <summary>
/// Contains data on initial island generation rules
/// - Biome
/// 
/// Contains data on island general information
/// - Trade input and output
/// - Allegiance data
/// 
/// - Check island render status, passes information to towns
/// </summary>

public class IslandMaster : MonoBehaviour
{
    public enum IslandBiome
    {
        Desert,
        Swamp,
        Jungle,
        DormantVolcano,
        ActiveVolcano,
        Tundra,
        Ethereal,
        Deadlands,
    }
    public IslandBiome islandBiome;     //This is set via gameobject generated with the island

    public Dictionary<GameObject, GameObject> SupplyIncomingRequests = new Dictionary<GameObject, GameObject>();
    public Dictionary<GameObject, GameObject> SupplyOutgoingRequests = new Dictionary<GameObject, GameObject>();

    public enum Allegiances
    {
        FactionA,
        FactionB,
        FactionC,
    }
    public Allegiances islandAllegiance;

    private MapMagicObject mmObject;
    private Graph terrainGraph;
    private MapMagic.Terrains.TerrainTile attachedTileStatus;

    public List<InitialTownGen> townsOnIsland = new List<InitialTownGen>();

    private Transform terrainTile;
    private static HashSet<string> initializedTiles = new HashSet<string>();
    private static Dictionary<string, int> tileSettlement = new Dictionary<string, int>();

    public BiomeScriptObjs biome;
    public bool resolved;

    private void Start()
    {
        if (isActiveAndEnabled)
        {
            terrainTile = transform.parent.parent;
            IsolateIslandMaster();

            var main = terrainTile.Find("Main Terrain");
            if (main != null)
            {
                main.gameObject.layer = LayerMask.NameToLayer("Terrain");

            }
            //Debug.Log("Tile status: " + terrainTile.GetComponent<MapMagic.Terrains.TerrainTile>().)
            //attachedTileStatus = transform.parent.parent.GetComponent<MapMagic.Terrains.TerrainTile>();
            //Not sure what to access to view changes right now -- Its in terrainTile script, but not sure what variable <-- this is related to the render status
        }
    }

    private void ApplyGround()
    {
        if (biome == null || biome.groundLayers == null || biome.groundLayers.Length == 0) return;
        var main = terrainTile.Find("Main Terrain");
        if (main == null) return;
        main.GetComponent<Terrain>().terrainData.terrainLayers = biome.groundLayers;
    }

    private void IsolateIslandMaster()
    {
        // Gather this tile's IMs, including this one
        mmObject = GetComponentInParent<MapMagicObject>();
        List<IslandMaster> candidates = new List<IslandMaster>();
        foreach (Transform t in transform.parent)
        {
            IslandMaster im = t.GetComponent<IslandMaster>();
            if (im != null) candidates.Add(im);
        }

        // Stable order: biome first, then position as a tiebreaker
        candidates.Sort((a, b) =>
        {
            int byBiome = a.islandBiome.CompareTo(b.islandBiome);
            if (byBiome != 0) return byBiome;
            return a.transform.position.x.CompareTo(b.transform.position.x);
        });

        // Seed from world seed + tile coordinate (integer math, stable across runs)
        Vector2Int tile = new Vector2Int(
            Mathf.FloorToInt(transform.position.x / mmObject.tileSize.x),
            Mathf.FloorToInt(transform.position.z / mmObject.tileSize.z));
        int seed = mmObject.graph.random.Seed ^ (tile.x * 73856093) ^ (tile.y * 19349663);

        // Group candidates by biome; candidates is already sorted, so insertion order is stable
        var groups = new Dictionary<IslandBiome, List<IslandMaster>>();
        foreach (var im in candidates)
        {
            if (!groups.TryGetValue(im.islandBiome, out var list))
                groups[im.islandBiome] = list = new List<IslandMaster>();
            list.Add(im);
        }

        // Biggest group wins (ties go to the first biome in sort order, deterministic)
        List<IslandMaster> best = null;
        foreach (var kv in groups)
            if (best == null || kv.Value.Count > best.Count) best = kv.Value;

        // Seeded pick inside the winning group
        IslandMaster winner = best[new System.Random(seed).Next(best.Count)];

        if (winner == this) { resolved = true; ApplyGround(); }
        else gameObject.SetActive(false);
    }
}
