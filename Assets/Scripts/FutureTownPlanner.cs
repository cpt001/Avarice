using UnityEngine;
using System.Collections.Generic;
using System.Collections;
/// <summary>
/// This scripts entire purpose is to act as a library that determines
/// - Master structure list of buildings belonging to town
/// - Build Order
/// - Build Location
/// - Demolition orders
/// 
/// Will need to access each tier of structure, determine list, then up tier and do again
/// 
/// Some structures are guaranteed spawns
/// - Some are guaranteed based on phenotype
/// 
/// </summary>
public class FutureTownPlanner : MonoBehaviour
{
    [SerializeField] private GameObject structureContainer;
    [SerializeField] private List<Transform> buildingWhitelist = new List<Transform>();
    [SerializeField] private List<Town_Building> BuildingQueue = new List<Town_Building>();

    //This sorts through all structures to get their probabilities
    public void GetPossibleStructures(Any_Town_Phenotype town_Phenotype)
    {
        structureContainer = GameObject.Find("Structures");
        foreach (Transform bldg in structureContainer.transform)
        {
            //Building sorted by phenotype spawn chances
            if (bldg.GetComponent<BuildingSpawnChanceData>())
            {
                foreach (KeyValuePair<Any_Town_Phenotype, int> kvp in bldg.GetComponent<BuildingSpawnChanceData>().phenotypeChanceDict)
                {
                   if (town_Phenotype == kvp.Key)
                   {
                        //This needs to be changed to generate per building instance, rather than the overall whitelist
                        //Remove the buildings that have 0 as the kvp value, then check each new structure for that chance, and move on to next in list if chance fails
                        if (kvp.Value != 0)
                        {
                            buildingWhitelist.Add(bldg.transform);
                        }
                   }
                }
            }
            //Building has no spawn chance data
            else
            {
                //Debug.Log("Building " + bldg.name + " has no SCD");
            }
        }
    }

    public void SetTownConstructionOrder(int internalTier, Any_Town_Phenotype town_Phenotype)
    {
        int buildingCount = 0;
        switch (internalTier)
        {
            case 0: { buildingCount = Mathf.RoundToInt(Random.Range(2, 6));     break; }
            case 1: { buildingCount = Mathf.RoundToInt(Random.Range(6, 9));     break; }
            case 2: { buildingCount = Mathf.RoundToInt(Random.Range(6, 10));    break; }
            case 3: { buildingCount = Mathf.RoundToInt(Random.Range(5, 12));    break; }
            case 4: { buildingCount = Mathf.RoundToInt(Random.Range(4, 8));     break; }
        }

        //Adds structures from selected tier
        List<Transform> buildingsInTier = new List<Transform>();
        buildingsInTier.Clear();
        foreach (Transform bldg in buildingWhitelist)
        {
            if (bldg.GetComponent<Town_Building>().BuildingTier == internalTier && !bldg.GetComponent<Town_Building>().isSpecialStructure)
            {
                buildingsInTier.Add(bldg);
            }
        }
        
        //Adds buildings up to the building count
        if (BuildingQueue.Count < buildingCount)
        {
            //Debug.Log("began queueing");
            for (int i = 0; i < buildingCount; i++)
            {
                int buildingRandomizer = Mathf.RoundToInt(Random.Range(0, buildingsInTier.Count));

                foreach (Transform bldg in buildingsInTier)
                {
                    if (bldg == buildingsInTier[buildingRandomizer])
                    {
                        foreach (KeyValuePair<Any_Town_Phenotype, int> buildingSCD in bldg.GetComponent<BuildingSpawnChanceData>().phenotypeChanceDict)
                        {
                            if (buildingSCD.Key == town_Phenotype)
                            {
                                //roll value against rand 0-11
                                int rand = Mathf.RoundToInt(Random.Range(0, 11));
                                if (buildingSCD.Value < rand)
                                {
                                    BuildingQueue.Add(bldg.GetComponent<Town_Building>());
                                }
                                else
                                {
                                    i--;
                                }
                            }
                        }
                    }
                }
            }
        }

        /*for (int i = 0; i < buildingCount; i++)
        {
            int buildingRandomizer = Mathf.RoundToInt(Random.Range(0, buildingsInTier.Count));
            if (buildingsInTier.Count != 0)
            {
                BuildingQueue.Add(buildingsInTier[buildingRandomizer]);
            }
            else
            {
                Debug.Log(transform + " has 0 buildings in tier " + internalTier);
            }
        }*/

        #region Old Implementation
        /*Debug.Log(internalTier + " int tier");
        //Check town max tier
        
        //Generate a number of required building to reach the next tier
        switch (internalTier)
        {
            case 0:
                {
                    int t0BuildingCount = Mathf.RoundToInt(Random.Range(5, 8));
                    List<Town_Building> buildingsInTier = new List<Town_Building>();
                    foreach (Town_Building bldg in buildingWhitelist)
                    {
                        if (bldg.BuildingTier == 0)
                        {
                            //Debug.Log("Adding " + bldg + " to tier " + internalTier);
                            buildingsInTier.Add(bldg);
                        }
                    }
                    for (int i = 0; i < t0BuildingCount; i++)
                    {
                        int buildingRandomizer = Mathf.RoundToInt(Random.Range(0, buildingsInTier.Count));
                        //Debug.Log("Randomizer " + buildingRandomizer + " bldg count:" + i);
                        if (buildingsInTier.Count != 0)
                        {
                            BuildingQueue.Add(buildingsInTier[buildingRandomizer]);     //Why is this out of range??
                        }
                        else
                        {
                            Debug.Log(gameObject + " has 0 buildings in tier!");
                        }
                    }
                    break;
                }
        }*/
        #endregion


        //Randomly select from buildings in each tier
        //Add housing to fill in resident cap per tier, order these in before building that requires it
        //Add demolition orders if necessary
        //Move to next tier until mex tier reached
        //Set and limit number of models to generate


        #region Town Phenotypes
        /*switch (town_Phenotype)
        {
            case Any_Town_Phenotype.Caravan_Site:
                {
                    break;
                }
            case Any_Town_Phenotype.Fertile_Island:
                {
                    break;
                }
            case Any_Town_Phenotype.Free_Port:
                {
                    break;
                }
            case Any_Town_Phenotype.Fishing_Village:
                {
                    break;
                }
            case Any_Town_Phenotype.Lighthouse_Keep:
                {
                    break;
                }
            case Any_Town_Phenotype.Industrial_Town:
                {
                    break;
                }
            case Any_Town_Phenotype.Mining_Colony:
                {
                    break;
                }
            case Any_Town_Phenotype.Mercantile_Trade_Port:
                {
                    break;
                }
            case Any_Town_Phenotype.Ship_Builders_Collective:
                {
                    break;
                }
            case Any_Town_Phenotype.Stronghold:
                {
                    break;
                }
            case Any_Town_Phenotype.Sailors_Respite:
                {
                    break;
                }
            case Any_Town_Phenotype.Swamp_Town:
                {
                    break;
                }
            case Any_Town_Phenotype.Penal_Colony:
                {
                    break;
                }
            case Any_Town_Phenotype.Ranchland:
                {
                    break;
                }
            case Any_Town_Phenotype.Wood_Shrouded:
                {
                    break;
                }
            case Any_Town_Phenotype.Native_Island:
                {
                    break;
                }
        }*/
        #endregion
    }

    private IEnumerator SetBuildingQueueForTier()
    {
        yield return null;
    }

    public void SetSpecialConstruction(int internalTier)
    {
        foreach (Transform bldg in buildingWhitelist)
        {
            if (bldg.GetComponent<Town_Building>().isSpecialStructure)
            {
                if (bldg.GetComponent<Town_Building>().BuildingTier == internalTier)
                {

                }
            }
        }
    }
}
