using UnityEngine;
using System.Collections.Generic;
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
    [SerializeField] private List<Town_Building> buildingWhitelist = new List<Town_Building>();
    private List<Town_Building> BuildingQueue = new List<Town_Building>();

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
                        //Debug.Log("Key match " + kvp.Key.ToString());
                        int queueAllowanceRandomNumber = Random.Range(0, 11);
                        if (queueAllowanceRandomNumber <= kvp.Value)
                        {
                            //Debug.Log("Value allowed");
                            buildingWhitelist.Add(bldg.GetComponent<Town_Building>());
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

    public void SetTownConstructionOrder(int townMaxTier)
    {
        int internalTier = 0;
        //Check town max tier
        
        //Generate a number of required building to reach the next tier
        switch (internalTier)
        {
            case 0:
                {
                    int t0Buildings = Mathf.RoundToInt(Random.Range(5, 8));
                    break;
                }
            case 1:
                {
                    int t1Buildings = Mathf.RoundToInt(Random.Range(8, 12));
                    break;
                }
            case 2:
                {
                    int t2Buildings = Mathf.RoundToInt(Random.Range(6, 10));
                    break;
                }
            case 3:
                {
                    int t3Buildings = Mathf.RoundToInt(Random.Range(5, 12));
                    break;
                }
            case 4:
                {
                    int t4Buildings = Mathf.RoundToInt(Random.Range(4, 8));
                    break;
                }
        }
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
}
