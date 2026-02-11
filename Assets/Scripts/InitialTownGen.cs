using UnityEngine;
using System.Collections.Generic;

public class InitialTownGen : MonoBehaviour
{
    /// <summary>
    /// Check island biome
    /// Check for additional town generators on island
    /// Determine tier
    /// - This also gives a bracket to the town's age
    /// Determine town phenotype
    /// - Check Far Horizons first for potential generation goals
    /// - If biome, tier, town position count, or size is outside of parameters, prevent ignore town phenotype preset
    /// - Some phenotypes cannot spawn in some biomes
    /// Determine structure composition
    /// Develop town core road system
    /// Update island allegiance
    /// Run economic model
    /// - Generate full personal supplies
    /// - Generate business supplies based on nearby island allegiances
    /// - Simulate recent piracy or weather events
    /// - Simulate crime rate based on supply count
    /// Determine population
    /// - Determine relationships, lineage, and family history
    /// - Graveyard population based on town age, recent incidents, and crime rate
    /// Determine island name
    /// Update island requester with needed and provided items 
    /// </summary>
    /// 

    private IslandMaster islandMaster => GameObject.FindWithTag("IslandMaster").GetComponent<IslandMaster>();
    private List<InitialTownGen> additionalTowns = new List<InitialTownGen>();
    public Town_Phenotype townType;

    private void GenerateTown()
    {
        if (additionalTowns.Count != 0)
        {
            //Tweaked data from other towns being generated
        }
        else
        {
            switch (islandMaster.islandBiome)
            {
                case IslandMaster.IslandBiome.Desert:
                    {
                        break;
                    }
                case IslandMaster.IslandBiome.Swamp:
                    {
                        break;
                    }
                case IslandMaster.IslandBiome.Jungle:
                    {
                        break;
                    }
                case IslandMaster.IslandBiome.DormantVolcano:
                    {
                        break;
                    }
                case IslandMaster.IslandBiome.ActiveVolcano:
                    {
                        break;
                    }
                case IslandMaster.IslandBiome.Tundra:
                    {
                        break;
                    }
                case IslandMaster.IslandBiome.Ethereal:
                    {
                        break;
                    }
                case IslandMaster.IslandBiome.Deadlands:
                    {
                        break;
                    }
            }
            townType = (Town_Phenotype)Random.Range(0, System.Enum.GetValues(typeof(Town_Phenotype)).Length);
        }
    }
}
