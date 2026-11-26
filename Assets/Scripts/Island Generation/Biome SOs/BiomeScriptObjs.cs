using UnityEngine;
using System.Collections;

[System.Serializable] public class WeightedPrefab { public GameObject prefab; public float weight = 1f; }
[System.Serializable] public class SlotTable { public NaturalSlot.SlotType natSlotType; public WeightedPrefab[] entries; }

[CreateAssetMenu(menuName = "Avarice/Biome")]
public class BiomeScriptObjs : ScriptableObject
{
    public TerrainLayer[] groundLayers;
    public SlotTable[] tables;

    public GameObject Pick(NaturalSlot.SlotType type, System.Random rng)
    {
        SlotTable table = null;
        foreach (var t in tables)
        {
            if (t.natSlotType == type) 
            { 
                table = t; 
                break; 
            }

        }
        if (table == null || table.entries == null || table.entries.Length == 0)
        {
            return null;
        }

        float total = 0f;
        foreach (var e in table.entries)
        {
            total += e.weight;
        }

        if (total <= 0f)
        {
            return null;
        }
        float roll = (float)(rng.NextDouble() * total);

        foreach (var e in table.entries)
        {
            roll -= e.weight;
            if (roll < 0f)
            {
                return e.prefab;
            }
        }
        return table.entries[table.entries.Length - 1].prefab;
    }
}
