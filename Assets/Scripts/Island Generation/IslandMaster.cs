using UnityEngine;
using System.Collections.Generic;

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

    private MapMagic.Terrains.TerrainTile attachedTileStatus;

    public List<InitialTownGen> townsOnIsland = new List<InitialTownGen>();

    private void Start()
    {
        //attachedTileStatus = transform.parent.parent.GetComponent<MapMagic.Terrains.TerrainTile>();
        //Not sure what to access to view changes right now -- Its in terrainTile script, but not sure what variable
    }
}
