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
    [SerializeField] private List<Town_Building> masterBuildingQueue = new List<Town_Building>();
    [SerializeField] private List<Town_Building> starterGenerationQueue = new List<Town_Building>();
    [SerializeField] private List<Town_Building> constructionOrders = new List<Town_Building>();
    [SerializeField] private int numberOfResidents;
    [SerializeField] private int numberOfWorkers;

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
        

        //->> might need to change generation to account for town slots on island instead of randomly picking a tier
        //->> that may also allow for higher building caps?
        //->> may be smart to simply split buildings between tiers instead
        //->> fishing huts need to be built near ocean
        //-->> find nearest beach point, drop dock and fishing huts on that
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
            case 0: { buildingCount = Mathf.RoundToInt(Random.Range(5, 9));  break; }
            case 1: { buildingCount += Mathf.RoundToInt(Random.Range(4, 6)); t0Additions = Mathf.RoundToInt(Random.Range(1, 4)); break; }
            case 2: { buildingCount += Mathf.RoundToInt(Random.Range(6, 10)); t1Additions = Mathf.RoundToInt(Random.Range(1, 4)); break; }
            case 3: { buildingCount += Mathf.RoundToInt(Random.Range(3, 7)); t2Additions = Mathf.RoundToInt(Random.Range(1, 4)); break; }
            case 4: { buildingCount += Mathf.RoundToInt(Random.Range(4, 8)); t3Additions = Mathf.RoundToInt(Random.Range(1, 4)); break; }
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
        if (masterBuildingQueue.Count < buildingCount)
        {
            int tryCount = 0;   //This is to attempt to create more unique structures
            for (int i = 0; i < buildingCount; i++)
            {
                int buildingRandomizer = Mathf.RoundToInt(Random.Range(0, buildingsInTier.Count));
                foreach (Transform bldg in buildingsInTier)
                {
                    if (bldg == buildingsInTier[buildingRandomizer])
                    {
                        if (!masterBuildingQueue.Contains(bldg.GetComponent<Town_Building>()) || masterBuildingQueue.Contains(bldg.GetComponent<Town_Building>()) && tryCount != 0)
                        {
                            foreach (KeyValuePair<Any_Town_Phenotype, int> buildingSCD in bldg.GetComponent<BuildingSpawnChanceData>().phenotypeChanceDict)
                            {
                                if (buildingSCD.Key == town_Phenotype)
                                {
                                    //roll value against rand 0-11
                                    int rand = Mathf.RoundToInt(Random.Range(0, 11));
                                    if (buildingSCD.Value < rand)
                                    {
                                        Town_Building targetBldg = bldg.GetComponent<Town_Building>();
                                        //Added for loop related to building resident count, to balance #houses vs residents
                                        if (targetBldg.buildingData.residentMax < targetBldg.buildingData.workerMax)
                                        {
                                            int adjustment = Mathf.RoundToInt((targetBldg.buildingData.workerMax - targetBldg.buildingData.residentMax) / 2);
                                            for (int n = 0; n > adjustment; n++)
                                            {
                                                AppendHousing();
                                            }
                                            if (adjustment == 0)
                                            {
                                                AppendHousing();
                                            }
                                        }
                                        masterBuildingQueue.Add(targetBldg);
                                        tryCount = 0;
                                    }
                                    else
                                    {
                                        i--;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Debug.Log("Reroll triggered");
                            tryCount++;
                            i--;
                        }
                    }
                }
            }
        }

        #region Retroactive addition after town tier up
        if (masterBuildingQueue.Count < masterBuildingQueue.Count + t0Additions)
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
                                //roll value against rand 0-11
                                int rand = Mathf.RoundToInt(Random.Range(0, 11));
                                if (buildingSCD.Value < rand)
                                {
                                    Town_Building targetBldg = bldg.GetComponent<Town_Building>();
                                    //Added for loop related to building resident count, to balance #houses vs residents
                                    if (targetBldg.buildingData.residentMax < targetBldg.buildingData.workerMax)
                                    {
                                        int adjustment = Mathf.RoundToInt((targetBldg.buildingData.workerMax - targetBldg.buildingData.residentMax) / 2);
                                        for (int n = 0; n > adjustment; n++)
                                        {
                                            AppendHousing();
                                        }
                                        if (adjustment == 0)
                                        {
                                            AppendHousing();
                                        }
                                    }
                                    masterBuildingQueue.Add(targetBldg);
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
        if (masterBuildingQueue.Count < masterBuildingQueue.Count + t1Additions)
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
                                //roll value against rand 0-11
                                int rand = Mathf.RoundToInt(Random.Range(0, 11));
                                if (buildingSCD.Value < rand)
                                {
                                    Town_Building targetBldg = bldg.GetComponent<Town_Building>();
                                    //Added for loop related to building resident count, to balance #houses vs residents
                                    if (targetBldg.buildingData.residentMax < targetBldg.buildingData.workerMax)
                                    {
                                        int adjustment = Mathf.RoundToInt((targetBldg.buildingData.workerMax - targetBldg.buildingData.residentMax) / 2);
                                        for (int n = 0; n > adjustment; n++)
                                        {
                                            AppendHousing();
                                        }
                                        if (adjustment == 0)
                                        {
                                            AppendHousing();
                                        }
                                    }
                                    masterBuildingQueue.Add(targetBldg);
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
        if (masterBuildingQueue.Count < masterBuildingQueue.Count + t2Additions)
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
                                //roll value against rand 0-11
                                int rand = Mathf.RoundToInt(Random.Range(0, 11));
                                if (buildingSCD.Value < rand)
                                {
                                    Town_Building targetBldg = bldg.GetComponent<Town_Building>();
                                    //Added for loop related to building resident count, to balance #houses vs residents
                                    if (targetBldg.buildingData.residentMax < targetBldg.buildingData.workerMax)
                                    {
                                        int adjustment = Mathf.RoundToInt((targetBldg.buildingData.workerMax - targetBldg.buildingData.residentMax) / 2);
                                        for (int n = 0; n > adjustment; n++)
                                        {
                                            AppendHousing();
                                        }
                                        if (adjustment == 0)
                                        {
                                            AppendHousing();
                                        }
                                    }
                                    masterBuildingQueue.Add(targetBldg);
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
        if (masterBuildingQueue.Count < masterBuildingQueue.Count + t3Additions)
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
                                //roll value against rand 0-11
                                int rand = Mathf.RoundToInt(Random.Range(0, 11));
                                if (buildingSCD.Value < rand)
                                {
                                    Town_Building targetBldg = bldg.GetComponent<Town_Building>();
                                    //Added for loop related to building resident count, to balance #houses vs residents
                                    if (targetBldg.buildingData.residentMax < targetBldg.buildingData.workerMax)
                                    {
                                        int adjustment = Mathf.RoundToInt((targetBldg.buildingData.workerMax - targetBldg.buildingData.residentMax) / 2);
                                        for (int n = 0; n > adjustment; n++)
                                        {
                                            AppendHousing();
                                        }
                                        if (adjustment == 0)
                                        {
                                            AppendHousing();
                                        }
                                    }
                                    masterBuildingQueue.Add(targetBldg);
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
        if (masterBuildingQueue.Count < masterBuildingQueue.Count + t4Additions)
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
                                //roll value against rand 0-11
                                int rand = Mathf.RoundToInt(Random.Range(0, 11));
                                if (buildingSCD.Value < rand)
                                {
                                    Town_Building targetBldg = bldg.GetComponent<Town_Building>();
                                    //Added for loop related to building resident count, to balance #houses vs residents
                                    if (targetBldg.buildingData.residentMax < targetBldg.buildingData.workerMax)
                                    {
                                        int adjustment = Mathf.RoundToInt((targetBldg.buildingData.workerMax - targetBldg.buildingData.residentMax) / 2);
                                        for (int n = 0; n > adjustment; n++)
                                        {
                                            AppendHousing();
                                        }
                                        if (adjustment == 0)
                                        {
                                            AppendHousing();
                                        }
                                    }
                                    masterBuildingQueue.Add(targetBldg);
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
        Transform houseObject = buildingWhitelist.Find(tran => tran.name == "House");
        masterBuildingQueue.Add(houseObject.GetComponent<Town_Building>());
        numberOfResidents += houseObject.GetComponent<Town_Building>().buildingData.residentMax;
    }

    //This needs to check physical space on the island.
    //If the total footprint will exceed the size, mark a structure for demolition
    //Priority -> extra houses, duplicates, then higher SCD structures
    public void SetDemolitionOrders()
    {
        numberOfResidents = 0;
        numberOfWorkers = 0;
        foreach (Town_Building bldg in masterBuildingQueue)
        {
            numberOfResidents += bldg.buildingData.residentMax;
            numberOfWorkers += bldg.buildingData.workerMax;
        }
        if (numberOfResidents > numberOfWorkers)
        {

        }

    }

    public void SetSpecialConstruction(int internalTier)
    {
        foreach (Transform bldg in buildingWhitelist)
        {
            if (bldg.GetComponent<Town_Building>().isSpecialStructure)
            {
                if (bldg.GetComponent<Town_Building>().BuildingTier == internalTier)
                {
                    if (!masterBuildingQueue.Contains(bldg.GetComponent<Town_Building>()))
                    {
                        masterBuildingQueue.Add(bldg.GetComponent<Town_Building>());
                    }
                }
            }
        }
    }

    //Places down the first structure, allowing the construction of a roadmap from it
    //Choose based on max possible tier, then utilize one of the advancement structures
    public void SetInitialStructure(int maxTier)
    {
        #region Choose random structure from advancement tiers [DEFUNCT]
        /*List<Transform> starterPossibilities = new List<Transform>();
        int rand = Mathf.RoundToInt(Random.Range(0, starterPossibilities.Count));

        foreach (Transform bldg in buildingWhitelist)
        {
            if (bldg.GetComponent<Town_Building>().isSpecialStructure && bldg.GetComponent<Town_Building>().BuildingTier == maxTier)
            {
                starterPossibilities.Add(bldg);
            }
        }
        if (starterPossibilities[rand] != null)
        {
            masterBuildingQueue.Add(starterPossibilities[rand].GetComponent<Town_Building>());
        }
        Debug.Log("Starter: " + starterPossibilities[rand]);
        */
        #endregion

        //Randomly decides between church and town square as origin
        int rand = Mathf.RoundToInt(Random.Range(0, 10));
        foreach (Transform bldg in buildingWhitelist)
        {
            if (rand <= 7)
            {
                if (bldg.name == "Town Square")
                {
                    masterBuildingQueue.Add(bldg.GetComponent<Town_Building>());
                    starterGenerationQueue.Add(bldg.GetComponent<Town_Building>());
                }
            }
            else
            {
                if (bldg.name == "Church")
                {
                    masterBuildingQueue.Add(bldg.GetComponent<Town_Building>());
                    starterGenerationQueue.Add(bldg.GetComponent<Town_Building>());
                    break;
                }
                else
                {
                    if (bldg.name == "Town Square")
                    {
                        masterBuildingQueue.Add(bldg.GetComponent<Town_Building>());
                    }
                }
            }
        }
    }

    public void GenerateInitialTown(int StarterTier, int maxTier)
    {
        List<Town_Building> buildingsAtTier = new List<Town_Building>();

        if (StarterTier != 0)
        {

        }
        else
        {
            Debug.Log("Starter T0");
            foreach (Town_Building bldg in masterBuildingQueue)
            {
                if (bldg.BuildingTier == 0)
                {
                    buildingsAtTier.Add(bldg);
                }
                constructionOrders.Add(bldg);
            }
            int builtCount = Mathf.RoundToInt(Random.Range(2, buildingsAtTier.Count));
            for (int i = 0; i < builtCount; i++)
            {
                if (buildingsAtTier[i] != null)
                {
                    starterGenerationQueue.Add(buildingsAtTier[i]);
                    constructionOrders.Remove(buildingsAtTier[i]);
                }
                else
                {
                    Debug.Log("BAT " + i + " is invalid, check GO, MBQ may be blank");
                }
            }
        }









        /*if (StarterTier != 0)
        {
            numConstructions = Mathf.RoundToInt(Random.Range(0, 3));
        }
        else
        {
            numConstructions = Mathf.RoundToInt(Random.Range(2, 5));
        }
        foreach (Town_Building bldg in masterBuildingQueue)
        {
            if (bldg.BuildingTier < StarterTier)
            {
                Debug.Log(bldg + " added to starterqueue");
                //Bounce through master queue. Anything under starterTier is automatically built. Anything at ST is evaluated. Anything over is ignored
                starterGenerationQueue.Add(bldg);

            }
            else if (bldg.BuildingTier == StarterTier)
            {
                //Generate with up to 3 under construction
                buildingsAtTier.Add(bldg);
            }
        }

        int numAlreadyEstablished = Mathf.RoundToInt(Random.Range(0, buildingsAtTier.Count));

        for (int i = 0; i > numAlreadyEstablished; i++)
        {
            Debug.Log(buildingsAtTier[i] + " added to starterqueue");
            starterGenerationQueue.Add(buildingsAtTier[i]);
        }
        for (int j = 0; j > numConstructions; j++)
        {
            starterGenerationQueue[starterGenerationQueue.Count - j].isUnderConstruction = true;
            Debug.Log("Construction: " + starterGenerationQueue[starterGenerationQueue.Count - j].isUnderConstruction);
        }*/
    }
}
