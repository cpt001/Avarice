// Editor-only: put this file in any folder named "Editor".
// Builds one TerrainLayer per biome colour map, paired with its shared normal map and tile size
// (the pairing table from the Avarice_BiomeTextures README).
// Usage: select the imported "Avarice_BiomeTextures" folder in the Project window, then
//        Tools > Avarice > Build Biome Terrain Layers.
// Output: <that folder>/TerrainLayers/<Biome>/<Name>.terrainlayer  (existing layers are updated in place)
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildBiomeTerrainLayers
{
    struct Pair { public string normal; public float tile; public Pair(string n, float t) { normal = n; tile = t; } }

    // Zones shared by every biome (matched on the part of the name after the biome prefix).
    static readonly Dictionary<string, Pair> ZoneRules = new Dictionary<string, Pair>
    {
        { "Beach",        new Pair("N_ripple",       3) },
        { "Cliff_Even",   new Pair("N_cliff_even",   8) },
        { "Cliff_Strata", new Pair("N_cliff_strata", 8) },
        { "Mountaintop",  new Pair("N_plates",       6) },
        { "Scree",        new Pair("N_pebbles",      3) },
    };

    // Flat-land variants differ per biome.
    static readonly Dictionary<string, Pair> FlatRules = new Dictionary<string, Pair>
    {
        { "AV_Flat_A", new Pair("N_soft", 5) },          { "AV_Flat_B", new Pair("N_blades_patchy", 4) }, { "AV_Flat_C", new Pair("N_cracked", 4) },
        { "DE_Flat_A", new Pair("N_ripple", 3) },        { "DE_Flat_B", new Pair("N_cracked", 4) },       { "DE_Flat_C", new Pair("N_blades_patchy", 4) },
        { "SW_Flat_A", new Pair("N_blades", 3) },        { "SW_Flat_B", new Pair("N_blades_patchy", 4) }, { "SW_Flat_C", new Pair("N_blades", 3) },
        { "JU_Flat_A", new Pair("N_blades", 3) },        { "JU_Flat_B", new Pair("N_leaves", 3) },        { "JU_Flat_C", new Pair("N_blades", 3) },
        { "TU_Flat_A", new Pair("N_soft", 5) },          { "TU_Flat_B", new Pair("N_blades_patchy", 4) }, { "TU_Flat_C", new Pair("N_blades", 3) },
        { "DV_Flat_A", new Pair("N_blades", 3) },        { "DV_Flat_B", new Pair("N_blades_patchy", 4) }, { "DV_Flat_C", new Pair("N_blades", 3) },
        { "ET_Flat_A", new Pair("N_blades", 3) },        { "ET_Flat_B", new Pair("N_blades_patchy", 4) }, { "ET_Flat_C", new Pair("N_blades", 3) },
        { "DL_Flat_A", new Pair("N_blades", 3) },        { "DL_Flat_B", new Pair("N_cracked", 4) },       { "DL_Flat_C", new Pair("N_blades_patchy", 4) },
    };

    [MenuItem("Tools/Avarice/Build Biome Terrain Layers")]
    static void Run()
    {
        string root = Selection.activeObject ? AssetDatabase.GetAssetPath(Selection.activeObject) : null;
        if (string.IsNullOrEmpty(root) || !AssetDatabase.IsValidFolder(root))
        {
            Debug.LogWarning("[TerrainLayers] Select the Avarice_BiomeTextures folder in the Project window first.");
            return;
        }

        // 1) Find the shared normals and make sure they import as normal maps.
        var normals = new Dictionary<string, Texture2D>();
        foreach (string guid in AssetDatabase.FindAssets("N_ t:Texture2D", new[] { root }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // FindAssets matches anywhere and ignores case ("Cliff_Even" contains "n_"), so check the prefix exactly.
            if (!Path.GetFileName(path).StartsWith("N_", System.StringComparison.Ordinal)) continue;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
            normals[Path.GetFileNameWithoutExtension(path)] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // 2) One TerrainLayer per colour map.
        int built = 0, missing = 0;
        foreach (string guid in AssetDatabase.FindAssets("_Albedo t:Texture2D", new[] { root }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path).Replace("_Albedo", ""); // e.g. JU_Flat_B_LeafLitter
            string prefix = name.Length > 2 ? name.Substring(0, 2) : name;                 // e.g. JU
            string rest = name.Length > 3 ? name.Substring(3) : "";                         // e.g. Flat_B_LeafLitter

            if (!TryGetPair(name, rest, out Pair pair) || !normals.TryGetValue(pair.normal, out Texture2D normal))
            {
                Debug.LogWarning($"[TerrainLayers] No pairing for {name}; skipped.");
                missing++;
                continue;
            }

            string biomeFolder = Path.GetFileName(Path.GetDirectoryName(path));
            string outDir = $"{root}/TerrainLayers/{biomeFolder}";
            EnsureFolder(outDir);
            string outPath = $"{outDir}/{name}.terrainlayer";

            var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(outPath);
            bool isNew = layer == null;
            if (isNew) layer = new TerrainLayer();

            layer.diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            layer.normalMapTexture = normal;
            layer.normalScale = 1f;
            layer.tileSize = new Vector2(pair.tile, pair.tile);
            layer.tileOffset = Vector2.zero;

            if (isNew) AssetDatabase.CreateAsset(layer, outPath);
            else EditorUtility.SetDirty(layer);
            built++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[TerrainLayers] Built or updated {built} layers. Unpaired: {missing}.");
    }

    static bool TryGetPair(string name, string rest, out Pair pair)
    {
        // Flat variants: match on "JU_Flat_B" etc.
        if (rest.StartsWith("Flat_") && name.Length >= 9 && FlatRules.TryGetValue(name.Substring(0, 9), out pair))
            return true;
        foreach (var kv in ZoneRules)
            if (rest == kv.Key) { pair = kv.Value; return true; }
        pair = default;
        return false;
    }

    static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
