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
    [SerializeField] private List<Transform> t0Bldg = new List<Transform>();
    [SerializeField] private List<Transform> t1Bldg = new List<Transform>();
    [SerializeField] private List<Transform> t2Bldg = new List<Transform>();
    [SerializeField] private List<Transform> t3Bldg = new List<Transform>();
    [SerializeField] private List<Transform> t4Bldg = new List<Transform>();
    [SerializeField] private List<Transform> t5Bldg = new List<Transform>();
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
                            switch (bldg.GetComponent<Town_Building>().BuildingTier)
                            {
                                case 0:
                                    {
                                        t0Bldg.Add(bldg);
                                        break;
                                    }
                                case 1:
                                    {
                                        t1Bldg.Add(bldg);
                                        break;
                                    }
                                case 2:
                                    {
                                        t2Bldg.Add(bldg);
                                        break;
                                    }
                                case 3:
                                    {
                                        t3Bldg.Add(bldg);
                                        break;
                                    }
                                case 4:
                                    {
                                        t4Bldg.Add(bldg);
                                        break;
                                    }
                                case 5:
                                    {
                                        t5Bldg.Add(bldg);
                                        break;
                                    }
                            }
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
        StartCoroutine(SetBuildingQueueForTier(internalTier, town_Phenotype));
        

        //->> might need to change generation to account for town slots instead of randomly picking a tier
        //->> that may also allow for higher building caps?
        //->> may be smart to simply split buildings between tiers instead
        //->> fishing huts need to be built near ocean
        //-->> find nearest beach point, drop dock and fishing huts on that
        //Add housing to fill in resident cap per tier, order these in before building that requires it
        //-Foreach loop through buildings in tier, and add necessary housing
        //Add demolition/move building orders if necessary
        //Set and limit number of models to generate for simulated growth
    }

    private IEnumerator SetBuildingQueueForTier(int internalTier, Any_Town_Phenotype town_Phenotype)
    {
        int buildingCount = 0;
        int t0Additions = 0;
        int t1Additions = 0;
        int t2Additions = 0;
        int t3Additions = 0;
        int t4Additions = 0;
        switch (internalTier)
        {
            case 0: { buildingCount = Mathf.RoundToInt(Random.Range(5, 9)); break; }
            case 1: { buildingCount += Mathf.RoundToInt(Random.Range(4, 6)); t0Additions = Mathf.RoundToInt(Random.Range(0, 3)); break; }
            case 2: { buildingCount += Mathf.RoundToInt(Random.Range(6, 10)); t1Additions = Mathf.RoundToInt(Random.Range(0, 3)); break; }
            case 3: { buildingCount += Mathf.RoundToInt(Random.Range(3, 7)); t2Additions = Mathf.RoundToInt(Random.Range(0, 3)); break; }
            case 4: { buildingCount += Mathf.RoundToInt(Random.Range(4, 8)); t3Additions = Mathf.RoundToInt(Random.Range(0, 3)); break; }
            case 5: { buildingCount += 1; t4Additions = Mathf.RoundToInt(Random.Range(0, 3)); break; }
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

        //Randomly selects and adds buildings up to the building count
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

        #region Retroactive addition after town tier up
        if (BuildingQueue.Count < BuildingQueue.Count + t0Additions)
        {
            for (int i = 0; i < t0Additions; i++)
            {
                int randBldg = Mathf.RoundToInt(Random.Range(0, t0Bldg.Count));
                foreach (Transform bldg in t0Bldg)
                {
                    if (bldg == t0Bldg[randBldg])
                    {
                        foreach (KeyValuePair<Any_Town_Phenotype, int> buildingSCD in bldg.GetComponent<BuildingSpawnChanceData>().phenotypeChanceDict)
                        {
                            if (buildingSCD.Key == town_Phenotype)
                            {
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
        if (BuildingQueue.Count < BuildingQueue.Count + t1Additions)
        {
            for (int i = 0; i < t1Additions; i++)
            {
                int randBldg = Mathf.RoundToInt(Random.Range(0, t1Bldg.Count));
                foreach (Transform bldg in t1Bldg)
                {
                    if (bldg == t1Bldg[randBldg])
                    {
                        foreach (KeyValuePair<Any_Town_Phenotype, int> buildingSCD in bldg.GetComponent<BuildingSpawnChanceData>().phenotypeChanceDict)
                        {
                            if (buildingSCD.Key == town_Phenotype)
                            {
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
        if (BuildingQueue.Count < BuildingQueue.Count + t2Additions)
        {
            for (int i = 0; i < t2Additions; i++)
            {
                int randBldg = Mathf.RoundToInt(Random.Range(0, t2Bldg.Count));
                foreach (Transform bldg in t2Bldg)
                {
                    if (bldg == t2Bldg[randBldg])
                    {
                        foreach (KeyValuePair<Any_Town_Phenotype, int> buildingSCD in bldg.GetComponent<BuildingSpawnChanceData>().phenotypeChanceDict)
                        {
                            if (buildingSCD.Key == town_Phenotype)
                            {
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
        if (BuildingQueue.Count < BuildingQueue.Count + t3Additions)
        {
            for (int i = 0; i < t3Additions; i++)
            {
                int randBldg = Mathf.RoundToInt(Random.Range(0, t3Bldg.Count));
                foreach (Transform bldg in t3Bldg)
                {
                    if (bldg == t3Bldg[randBldg])
                    {
                        foreach (KeyValuePair<Any_Town_Phenotype, int> buildingSCD in bldg.GetComponent<BuildingSpawnChanceData>().phenotypeChanceDict)
                        {
                            if (buildingSCD.Key == town_Phenotype)
                            {
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
        if (BuildingQueue.Count < BuildingQueue.Count + t4Additions)
        {
            for (int i = 0; i < t4Additions; i++)
            {
                int randBldg = Mathf.RoundToInt(Random.Range(0, t4Bldg.Count));
                foreach (Transform bldg in t4Bldg)
                {
                    if (bldg == t4Bldg[randBldg])
                    {
                        foreach (KeyValuePair<Any_Town_Phenotype, int> buildingSCD in bldg.GetComponent<BuildingSpawnChanceData>().phenotypeChanceDict)
                        {
                            if (buildingSCD.Key == town_Phenotype)
                            {
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
        #endregion
        yield return null;
    }

    //With each building added, check the housing deficit. 
    //If the available resident slots are below worker slots, add a house
    //If the total resident slots exceeds the needed housing, mark house for demolition
    private void AppendHousing()
    {
        
    }

    //This needs to check physical space on the island.
    //If the total footprint will exceed the size, mark a structure for demolition
    //Priority -> duplicates, then higher SCD structures
    private void MarkDemolitionOrders()
    {

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

    //Places down the first structure, allowing the construction of a roadmap from it
    public void SetInitialStructure()
    {

    }
}
