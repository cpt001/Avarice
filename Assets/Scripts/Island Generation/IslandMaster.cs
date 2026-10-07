using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MapMagic.Core;
using MapMagic.Terrains;
using MapMagic.Nodes;
using System.Collections;
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

    private void Start()
    {
        StartCoroutine(DelayedGeneration());

        //Debug.Log("Targeted parent: " + transform.parent.parent);
        terrainTile = transform.parent.parent;
        terrainTile.Find("Main Terrain").gameObject.layer = LayerMask.NameToLayer("Terrain");
        //Debug.Log("Tile status: " + terrainTile.GetComponent<MapMagic.Terrains.TerrainTile>().)
        //attachedTileStatus = transform.parent.parent.GetComponent<MapMagic.Terrains.TerrainTile>();
        //Not sure what to access to view changes right now -- Its in terrainTile script, but not sure what variable <-- this is related to the render status
    }

    private IEnumerator DelayedGeneration()
    {
        yield return new WaitForEndOfFrame();

        mmObject = GetComponentInParent<MapMagicObject>();
        terrainGraph = mmObject.graph;

        Coord tileCoord = Coord.Floor(
            transform.position.x / mmObject.tileSize.x,
            transform.position.z / mmObject.tileSize.z);

        string tileKey = $"{tileCoord.x}_{tileCoord.z}";

        // Respawned after StartGenerate, or sibling biome already claimed this tile
        if (initializedTiles.Contains(tileKey))
        {
            terrainGraph.defaults["isSettledConst"] = tileSettlement[tileKey];
            yield break;
        }

        initializedTiles.Add(tileKey);

        bool isSettled = Random.Range(0, 2) == 1;
        tileSettlement[tileKey] = isSettled ? 1 : 0;
        terrainGraph.defaults["isSettledConst"] = isSettled ? 1 : 0;
        Debug.Log($"[IM] isSettledConst reads back as: {terrainGraph.defaults["isSettledConst"]}");

        Debug.Log($"[IM] Tile {tileKey} -> {(isSettled ? "SETTLED" : "natural")}");

        if (isSettled)
        {
            TerrainTile tt = mmObject.tiles[tileCoord];
            if (tt != null)
                tt.StartGenerate(terrainGraph, true, true);
        }

        yield return null;
    }
}
