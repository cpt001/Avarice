using UnityEngine;
using System.Collections;

public class NaturalSlot : MonoBehaviour
{
    public enum SlotType
    {
        Tree,
        Stand,
        Plant,
        HarvestablePlant,
        Rock,
        Scree,
        Quirk,
        Habitat,
        Vent
    }
    public SlotType slotType;

    private void OnEnable() => StartCoroutine(WaitForIM());

    private void OnDisable()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private IEnumerator WaitForIM()
    {
        IslandMaster im = null;
        while (im == null)
        {
            im = FindResolvedIM();
            if (im == null) yield return new WaitForSeconds(0.2f);
        }
        Fill(im.biome);
    }

    private void Fill(BiomeScriptObjs b)
    {
        if (b == null) return;
        Vector3 p = transform.position;
        int hash = (Mathf.RoundToInt(p.x * 10f) * 73856093) ^ (Mathf.RoundToInt(p.z * 10f) * 19349663);
        GameObject prefab = b.Pick(slotType, new System.Random(hash));
        if (prefab != null) Instantiate(prefab, transform);
    }

    private IslandMaster FindResolvedIM()
    {
        var tile = GetComponentInParent<MapMagic.Terrains.TerrainTile>();
        if (tile == null) return null;
        foreach (var im in tile.GetComponentsInChildren<IslandMaster>())   // skips inactive losers by default
            if (im.resolved) return im;
        return null;
    }
}
