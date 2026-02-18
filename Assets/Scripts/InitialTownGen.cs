using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FutureTownPlanner))]
public class InitialTownGen : MonoBehaviour
{
    /// <summary>
    /// [x] Check island biome
    /// [x] Check for additional town generators on island
    /// [x] Determine tier
    /// [x] - This also gives a bracket to the town's age
    /// [x] Determine town phenotype
    /// - Check Far Horizons first for potential generation goals
    /// - If biome, tier, town position count, or size is outside of parameters, prevent ignore town phenotype preset
    /// [x] - Some phenotypes cannot spawn in some biomes
    /// Determine structure composition and future construction order
    /// - On roads: two future methods exist.
    /// -- Generate road grid beforehand, with building points attached to the roadside. Generates as a full grid, and sets up for slots. May not leave the room for all building sizes
    /// -- Generate single building, with points attached to the front of it. Find angle, generate next building from road point in front
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

    public IslandMaster islandMaster => GameObject.FindWithTag("IslandMaster").GetComponent<IslandMaster>();
    public Any_Town_Phenotype townType;

    private int setTownTier;        //Sets the town's possible generation tier.
    private float townAge;          //Sets a town's age, for historical purposes

    private FutureTownPlanner associatedTownPlanner => GetComponent<FutureTownPlanner>();

    private void GenerateTown()
    {
        #region Set Initial Detail
        if (islandMaster.townsOnIsland.Count != 0)
        {

            //Get data from associated towns
        }
        else
        {
            switch (islandMaster.islandBiome)
            {
                case IslandMaster.IslandBiome.Desert:
                    {
                        setTownTier = Mathf.RoundToInt(Random.Range(0, 4));
                        var desertPhenotype = (DesertPhenotypes)Random.Range(0, System.Enum.GetValues(typeof(Any_Town_Phenotype)).Length);
                        //Above sets from limited enum, and below sets to universal enum (Selection: CS, FP, FV, NI)
                        switch (desertPhenotype)
                        {
                            case DesertPhenotypes.Caravan_Site:     { townType = Any_Town_Phenotype.Caravan_Site;       break; }
                            case DesertPhenotypes.Fishing_Village:  { townType = Any_Town_Phenotype.Fishing_Village;    break; }
                            case DesertPhenotypes.Free_Port:        { townType = Any_Town_Phenotype.Free_Port;          break; }
                            case DesertPhenotypes.Native_Island:    { townType = Any_Town_Phenotype.Native_Island;      break; }
                        }
                        break;
                    }
                case IslandMaster.IslandBiome.Swamp:
                    {
                        setTownTier = Mathf.RoundToInt(Random.Range(0, 4));
                        var swampPhenotype = (SwampPhenotypes)Random.Range(0, System.Enum.GetValues(typeof(SwampPhenotypes)).Length);
                        //CS, FP, LK, St, ST, WS, NI
                        switch (swampPhenotype)
                        {
                            case SwampPhenotypes.Caravan_Site:      { townType = Any_Town_Phenotype.Caravan_Site;       break; }
                            case SwampPhenotypes.Free_Port:         { townType = Any_Town_Phenotype.Free_Port;          break; }
                            case SwampPhenotypes.Lighthouse_Keep:   { townType = Any_Town_Phenotype.Lighthouse_Keep;    break; }
                            case SwampPhenotypes.Stronghold:        { townType = Any_Town_Phenotype.Stronghold;         break; }
                            case SwampPhenotypes.Swamp_Town:        { townType = Any_Town_Phenotype.Swamp_Town;         break; }
                            case SwampPhenotypes.Wood_Shrouded:     { townType = Any_Town_Phenotype.Wood_Shrouded;      break; }
                            case SwampPhenotypes.Native_Island:     { townType = Any_Town_Phenotype.Native_Island;      break; }
                        }
                        break;
                    }
                case IslandMaster.IslandBiome.Jungle:
                    {
                        setTownTier = Mathf.RoundToInt(Random.Range(0, 5));
                        //CS, FI, FP, FV, LK, IT, MTP, SBC, St, SR, RL, WS, NI
                        break;
                    }
                case IslandMaster.IslandBiome.DormantVolcano:
                    {
                        setTownTier = Mathf.RoundToInt(Random.Range(0, 5));
                        //CS, FI, FP, IT, MC, MTP, SBC, St, SR, RL, WS, NI
                        break;
                    }
                case IslandMaster.IslandBiome.ActiveVolcano:
                    {
                        setTownTier = Mathf.RoundToInt(Random.Range(0, 4));
                        //CS, FP, FV, LK, IT, MC, St, PC, NI
                        break;
                    }
                case IslandMaster.IslandBiome.Tundra:
                    {
                        setTownTier = Mathf.RoundToInt(Random.Range(0, 3));
                        //FP, FV, LK, MTP, St, SR, PC, NI
                        break;
                    }
                case IslandMaster.IslandBiome.Ethereal:
                    {
                        setTownTier = Mathf.RoundToInt(Random.Range(0, 5));
                        //CS, FP, FV, MC, LK, MTP, St, PC, WS, NI
                        break;
                    }
                case IslandMaster.IslandBiome.Deadlands:    //Town will generate dead
                    {
                        setTownTier = Mathf.RoundToInt(Random.Range(0, 5));
                        //FP, FV, LK, MC, MTP, SBC, St, PC, RL, WS, NI
                        break;
                    }
            }
            townType = (Any_Town_Phenotype)Random.Range(0, System.Enum.GetValues(typeof(Any_Town_Phenotype)).Length);
        }
        #endregion
        //Set town age based on tier
        if (setTownTier == 0)
        {
            townAge = Random.Range(2, 30);
        }
        else if (setTownTier == 1) 
        {
            townAge = Random.Range(10, 70);
        }
        else if (setTownTier == 2) 
        {
            townAge = Random.Range(30, 60);
        }
        else if (setTownTier == 3) 
        {
            townAge = Random.Range(60, 120);
        }
        else if (setTownTier == 4) 
        {
            townAge = Random.Range(90, 170);
        }
        else if (setTownTier == 5) 
        {
            townAge = Random.Range(140, 220);
        }
        //Access town planner script

        //Allegiance model
        //Economy model
        //Populate
        //Name Island
        //Update IslandMaster with requests/supplies
    }
}
