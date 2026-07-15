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
/// 
/// 
/// =->> might need to change generation to account for town slots on island instead of randomly picking a tier
/// ->> that may also allow for higher building caps?
/// ->> may be smart to simply split buildings between tiers instead
/// ->> fishing huts need to be built near ocean
/// -->> find nearest beach point, drop dock and fishing huts on that
/// Add demolition/move building orders if necessary
/// 
/// Fort needs to reduce town max tier by 1
/// </summary>
public class FutureTownPlanner : MonoBehaviour
{
    [SerializeField] private bool useDebugBuildings = false;
    [SerializeField] private IslandMaster islandMaster;
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
    [SerializeField] private List<GameObject> adjustedBuildings = new List<GameObject>();
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
                                        if (!bldg.GetComponent<Town_Building>().isSpecialStructure) { t0Bldg.Add(bldg); }
                                        break;
                                    }
                                case 1:
                                    {
                                        if (!bldg.GetComponent<Town_Building>().isSpecialStructure) { t1Bldg.Add(bldg); }
                                        break;
                                    }
                                case 2:
                                    {
                                        if (!bldg.GetComponent<Town_Building>().isSpecialStructure) { t2Bldg.Add(bldg); }
                                        break;
                                    }
                                case 3:
                                    {
                                        if (!bldg.GetComponent<Town_Building>().isSpecialStructure) { t3Bldg.Add(bldg); }
                                        break;
                                    }
                                case 4:
                                    {
                                        if (!bldg.GetComponent<Town_Building>().isSpecialStructure) { t4Bldg.Add(bldg); }
                                        break;
                                    }
                                case 5:
                                    {
                                        if (!bldg.GetComponent<Town_Building>().isSpecialStructure) { t5Bldg.Add(bldg); }
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

    public void SetTownConstructionOrder(int internalTier, Any_Town_Phenotype town_Phenotype, int maxTier)
    {
        StartCoroutine(SetBuildingQueueForTier(internalTier, town_Phenotype, maxTier));
    }

    private IEnumerator SetBuildingQueueForTier(int internalTier, Any_Town_Phenotype town_Phenotype, int maxTier)
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
            case 1: { buildingCount += Mathf.RoundToInt(Random.Range(4, 6)); t0Additions = Mathf.RoundToInt(Random.Range(1, 4)); break; }
            case 2: { buildingCount += Mathf.RoundToInt(Random.Range(6, 10)); t1Additions = Mathf.RoundToInt(Random.Range(1, 4)); break; }
            case 3: { buildingCount += Mathf.RoundToInt(Random.Range(3, 7)); t2Additions = Mathf.RoundToInt(Random.Range(1, 4)); break; }
            case 4: { buildingCount += Mathf.RoundToInt(Random.Range(4, 8)); t3Additions = Mathf.RoundToInt(Random.Range(1, 4)); break; }
            case 5: { buildingCount += 1; t4Additions = Mathf.RoundToInt(Random.Range(0, 3)); break; }
        }
        List<Transform> buildingsInTier = new List<Transform>();

        //Adds structures from selected tier
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
                                        if (i != 0)
                                        {
                                            i--;
                                        }
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
    //If the total resident slots exceeds the needed housing, mark house for demolition -- TBI
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
    public void RelocateBuilding()
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
            int builtCount = Mathf.RoundToInt(Random.Range(0, buildingsAtTier.Count));

            foreach (Town_Building bldg in masterBuildingQueue)
            {
                constructionOrders.Add(bldg);

                if (bldg.BuildingTier < StarterTier)
                {
                    starterGenerationQueue.Add(bldg);
                    constructionOrders.Remove(bldg);
                }
                else if (bldg.BuildingTier == StarterTier)
                {
                    buildingsAtTier.Add(bldg);
                }
            }
            for (int i = 0; i < builtCount; i++)
            {
                if (buildingsAtTier[i] != null)
                {
                    starterGenerationQueue.Add(buildingsAtTier[i]);
                    constructionOrders.Remove(buildingsAtTier[i]);
                }
                else
                {
                    Debug.Log("BAT " + i + " is invalid, check GO, MBQ may be blank", gameObject);
                }
            }
        }
        else
        {
            //This code can be recycled for buildings at the maximum tier. 
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
    }

    public void SpawnStructures()
    {
        foreach (GameObject ims in GameObject.FindGameObjectsWithTag("IslandMaster"))
        {
            if (ims.transform.parent == transform.parent)
            {
                islandMaster = ims.GetComponent<IslandMaster>();
            }
        }
        StartCoroutine(SpawnStructuresCoroutine());
    }
    private IEnumerator SpawnStructuresCoroutine()
    {
        foreach (Town_Building bldg in masterBuildingQueue)
        {
            foreach (Transform t in buildingWhitelist)
            {
                if (bldg.name == t.name)
                {
                    GameObject g = Instantiate(t.gameObject, transform.position, transform.rotation, gameObject.transform);

                    #region Building model identity
                    if (!useDebugBuildings)
                    {
                        switch (islandMaster.islandBiome)
                        {
                            case IslandMaster.IslandBiome.Desert:
                                {
                                    GameObject bldgModel = t.GetComponent<Town_Building>().buildingData.DesertModelSet;
                                    Instantiate(bldgModel, g.transform.position, g.transform.rotation, g.transform);
                                    break;
                                }
                            case IslandMaster.IslandBiome.Swamp:
                                {
                                    GameObject bldgModel = t.GetComponent<Town_Building>().buildingData.SwampModelSet;
                                    Instantiate(bldgModel, g.transform.position, g.transform.rotation, g.transform);
                                    break;
                                }
                            case IslandMaster.IslandBiome.Jungle:
                                {
                                    GameObject bldgModel = t.GetComponent<Town_Building>().buildingData.JungleModelSet;
                                    Instantiate(bldgModel, g.transform.position, g.transform.rotation, g.transform);
                                    break;
                                }
                            case IslandMaster.IslandBiome.DormantVolcano:
                                {
                                    GameObject bldgModel = t.GetComponent<Town_Building>().buildingData.DormantVolcanoModelSet;
                                    Instantiate(bldgModel, g.transform.position, g.transform.rotation, g.transform);
                                    break;
                                }
                            case IslandMaster.IslandBiome.ActiveVolcano:
                                {
                                    GameObject bldgModel = t.GetComponent<Town_Building>().buildingData.ActiveVolcanicModelSet;
                                    Instantiate(bldgModel, g.transform.position, g.transform.rotation, g.transform);
                                    break;
                                }
                            case IslandMaster.IslandBiome.Tundra:
                                {
                                    GameObject bldgModel = t.GetComponent<Town_Building>().buildingData.TundraModelSet;
                                    Instantiate(bldgModel, g.transform.position, g.transform.rotation, g.transform);
                                    break;
                                }
                            case IslandMaster.IslandBiome.Ethereal:
                                {
                                    GameObject bldgModel = t.GetComponent<Town_Building>().buildingData.EtherealModelSet;
                                    Instantiate(bldgModel, g.transform.position, g.transform.rotation, g.transform);
                                    break;
                                }
                            case IslandMaster.IslandBiome.Deadlands:
                                {
                                    Debug.Log("Deadlands spawn; NYI, needs to pick randomly from another model set");
                                    break;
                                }
                        }
                    }
                    else
                    {

                        if (t.GetComponent<Town_Building>().buildingData.debugObject)
                        {
                            GameObject bldgModel = t.GetComponent<Town_Building>().buildingData.debugObject;
                            Instantiate(bldgModel, g.transform.position, g.transform.rotation, g.transform);

                            //May be unneeded
                            foreach (BuildingSlot slot in bldgModel.transform.GetComponentsInChildren<BuildingSlot>())
                            {
                                slot.slotStatus = BuildingSlot.SlotFillEnum.Unavailable;
                            }
                        }
                    }
                    #endregion

                    #region Building Attachment and Adjustment
                    if (bldg == masterBuildingQueue[0])
                    {
                        //Debug.Log("MBQ/0: " + bldg);
                        float rand = Random.Range(-180, 180);
                        Quaternion randomRot = Quaternion.Euler(0, rand, 0);
                        transform.rotation = randomRot;
                        adjustedBuildings.Add(g);
                        SlotBuilder slotBuilder = g.GetComponentInChildren<SlotBuilder>();
                        slotBuilder.distanceFromCenter = 0;
                        slotBuilder.BuildStructureSlots(true);
                    }
                    else
                    {
                        List<GameObject> availableSlots = new List<GameObject>();
                        switch (bldg.setupCondition)
                        {
                            case Town_Building.SetupCondition.Beach:
                                {
                                    //Structures that are better served by remaining on the beach: docks, fishing huts etc
                                    //Map magic's built in randomizer can serve as placement points for these, but will need to be retrofitted for this task
                                    break;
                                }
                            case Town_Building.SetupCondition.Standalone:
                                {
                                    //This is reserved for structures that dont belong to part of the city: forts and such
                                    //These will be spawned at another town setup location
                                    break;
                                }
                            case Town_Building.SetupCondition.CityScape:
                                {
                                    List<Transform> slots = new List<Transform>();
                                    foreach (GameObject adjustedStructure in adjustedBuildings)
                                    {
                                        SlotBuilder adjStrSB = adjustedStructure.GetComponentInChildren<SlotBuilder>();
                                        //access slots
                                        for (int i = 0; i < adjStrSB.slots.Count; i++)
                                        {
                                            slots.Add(adjStrSB.slots[i].transform);
                                        }

                                        while (true)
                                        {
                                            bool placementAndRotationFinalized = false;
                                            int randomSlot = Random.Range(0, slots.Count);
                                            Transform trySlot = slots[randomSlot];
                                            List<Quaternion> validRots = new List<Quaternion>();
                                            SlotBuilder sb = null;

                                            if (slots.Count != 0)
                                            {
                                                if (g.GetComponentInChildren<SlotBuilder>())
                                                {
                                                    sb = g.GetComponentInChildren<SlotBuilder>();
                                                    for (int i = 0; i < 24; i++)
                                                    {
                                                        Quaternion rotation = Quaternion.Euler(0, i * 15, 0);
                                                        g.transform.SetLocalPositionAndRotation(trySlot.localPosition, rotation);
                                                        sb.CheckCollider();
                                                        if (!sb.triggerDetectingObject)
                                                        {
                                                            validRots.Add(g.transform.rotation);
                                                        }
                                                        else
                                                        {
                                                            //may be better methodology to remove spawn points intersecting building collider instead
                                                            Debug.Log("SB detected something; continuing loop");
                                                            continue;
                                                        }
                                                    }
                                                    if (validRots.Count != 0)
                                                    {
                                                        int randFinalRot = Random.Range(0, validRots.Count);
                                                        Quaternion rotation = validRots[randFinalRot];
                                                        g.transform.SetLocalPositionAndRotation(trySlot.localPosition, rotation);
                                                        //This breaks the loop in a big way
                                                        //adjustedBuildings.Add(g);
                                                        placementAndRotationFinalized = true;
                                                    }
                                                    else
                                                    {
                                                        //Try a different slot
                                                        //slots.RemoveAt(randomSlot);
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    Debug.Log("Missing SB: " + g, g);
                                                    break;
                                                }
                                                if (placementAndRotationFinalized == true)
                                                {
                                                    break;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                Debug.Log("All slots filled, or an issue has occured. Loop failure.");
                                                break;
                                            }
                                        }
                                    }
                                    //t.GetComponent<SlotBuilder>().
                                    break;
                                }
                        }

                        if (starterGenerationQueue.Contains(bldg))
                        {
                            t.gameObject.SetActive(true);
                        }
                        if (constructionOrders.Contains(bldg))
                        {
                            t.gameObject.SetActive(true);
                        }
                    }

                    #endregion
                }
            }
        }
        yield return null;
    }
}
