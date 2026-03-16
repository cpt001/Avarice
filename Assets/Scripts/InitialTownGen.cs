using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
    /// --> Generate single building, with points attached to the front of it. Find angle, generate next building from road point in front
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

    public IslandMaster islandMaster;// => GameObject.FindWithTag("IslandMaster").GetComponent<IslandMaster>();
    public Any_Town_Phenotype townType;

    private int setTownTier;        //Sets the town's possible generation tier.
    private float townAge;          //Sets a town's age, for historical purposes

    private FutureTownPlanner associatedTownPlanner => GetComponent<FutureTownPlanner>();

    private void Start()
    {
        foreach (GameObject ims in GameObject.FindGameObjectsWithTag("IslandMaster"))
        {
            if (ims.transform.parent == transform.parent)
            {
                islandMaster = ims.GetComponent<IslandMaster>();
            }
        }
        islandMaster.townsOnIsland.Add(this);
        StartCoroutine(GenerateTown());
    }

    private IEnumerator GenerateTown()
    {
        #region Set Initial Detail
        if (islandMaster.townsOnIsland.Count != 0)
        {
            //Get data from associated towns
            //Debug.Log("Islandmaster interference; Origin town " + islandMaster.townsOnIsland[0]);
            if (islandMaster.townsOnIsland[0] == this)
            {
                SetTownFromPhenotype();
            }
            else
            {
                townType = islandMaster.townsOnIsland[0].townType;
                gameObject.SetActive(false);    //Just for now during diagnostics
                //transform.parent = islandMaster.townsOnIsland[0].transform;
                //Future town planner needs to account for multiple town locations
            }
        }
        else
        {
            SetTownFromPhenotype();
        }
        #endregion
        //Set town age based on tier
        if (setTownTier == 0) { townAge = Random.Range(2, 30); }
        else if (setTownTier == 1) { townAge = Random.Range(10, 70); }
        else if (setTownTier == 2) { townAge = Random.Range(30, 60); }
        else if (setTownTier == 3) { townAge = Random.Range(60, 120); }
        else if (setTownTier == 4) { townAge = Random.Range(90, 170); }
        else if (setTownTier == 5) { townAge = Random.Range(140, 220); }
        
        yield return new WaitForSeconds(1);
        //Access town planner script
        StartTownPlanning();
        //Allegiance model
        //Economy model
        //Populate
        //Name Island
        //Update IslandMaster with requests/supplies

        yield return null;
    }

    void SetTownFromPhenotype()
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
                        case DesertPhenotypes.Caravan_Site: { townType = Any_Town_Phenotype.Caravan_Site; break; }
                        case DesertPhenotypes.Fishing_Village: { townType = Any_Town_Phenotype.Fishing_Village; break; }
                        case DesertPhenotypes.Free_Port: { townType = Any_Town_Phenotype.Free_Port; break; }
                        case DesertPhenotypes.Native_Island: { townType = Any_Town_Phenotype.Native_Island; break; }
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
                        case SwampPhenotypes.Caravan_Site: { townType = Any_Town_Phenotype.Caravan_Site; break; }
                        case SwampPhenotypes.Free_Port: { townType = Any_Town_Phenotype.Free_Port; break; }
                        case SwampPhenotypes.Lighthouse_Keep: { townType = Any_Town_Phenotype.Lighthouse_Keep; break; }
                        case SwampPhenotypes.Stronghold: { townType = Any_Town_Phenotype.Stronghold; break; }
                        case SwampPhenotypes.Swamp_Town: { townType = Any_Town_Phenotype.Swamp_Town; break; }
                        case SwampPhenotypes.Wood_Shrouded: { townType = Any_Town_Phenotype.Wood_Shrouded; break; }
                        case SwampPhenotypes.Native_Island: { townType = Any_Town_Phenotype.Native_Island; break; }
                    }
                    break;
                }
            case IslandMaster.IslandBiome.Jungle:
                {
                    setTownTier = Mathf.RoundToInt(Random.Range(0, 5));
                    var junglePhenotype = (JunglePhenotypes)Random.Range(0, System.Enum.GetValues(typeof(JunglePhenotypes)).Length);
                    //CS, FI, FP, FV, LK, IT, MTP, SBC, St, SR, RL, WS, NI
                    switch (junglePhenotype)
                    {
                        case JunglePhenotypes.Caravan_Site: { townType = Any_Town_Phenotype.Caravan_Site; break; }
                        case JunglePhenotypes.Fertile_Island: { townType = Any_Town_Phenotype.Fertile_Island; break; }
                        case JunglePhenotypes.Free_Port: { townType = Any_Town_Phenotype.Free_Port; break; }
                        case JunglePhenotypes.Fishing_Village: { townType = Any_Town_Phenotype.Fishing_Village; break; }
                        case JunglePhenotypes.Lighthouse_Keep: { townType = Any_Town_Phenotype.Lighthouse_Keep; break; }
                        case JunglePhenotypes.Industrial_Town: { townType = Any_Town_Phenotype.Industrial_Town; break; }
                        case JunglePhenotypes.Mercantile_Trade_Port: { townType = Any_Town_Phenotype.Mercantile_Trade_Port; break; }
                        case JunglePhenotypes.Ship_Builders_Collective: { townType = Any_Town_Phenotype.Ship_Builders_Collective; break; }
                        case JunglePhenotypes.Stronghold: { townType = Any_Town_Phenotype.Stronghold; break; }
                        case JunglePhenotypes.Sailors_Respite: { townType = Any_Town_Phenotype.Sailors_Respite; break; }
                        case JunglePhenotypes.Ranchland: { townType = Any_Town_Phenotype.Ranchland; break; }
                        case JunglePhenotypes.Wood_Shrouded: { townType = Any_Town_Phenotype.Wood_Shrouded; break; }
                        case JunglePhenotypes.Native_Island: { townType = Any_Town_Phenotype.Native_Island; break; }
                    }
                    break;
                }
            case IslandMaster.IslandBiome.DormantVolcano:
                {
                    setTownTier = Mathf.RoundToInt(Random.Range(0, 5));
                    var dormantVolcanoPhenotypes = (DormantVolcanoPhenotypes)Random.Range(0, System.Enum.GetValues(typeof(DormantVolcanoPhenotypes)).Length);
                    //CS, FI, FP, IT, MC, MTP, SBC, St, SR, RL, WS, NI
                    switch (dormantVolcanoPhenotypes)
                    {
                        case DormantVolcanoPhenotypes.Caravan_Site: { townType = Any_Town_Phenotype.Caravan_Site; break; }
                        case DormantVolcanoPhenotypes.Fertile_Island: { townType = Any_Town_Phenotype.Fertile_Island; break; }
                        case DormantVolcanoPhenotypes.Free_Port: { townType = Any_Town_Phenotype.Free_Port; break; }
                        case DormantVolcanoPhenotypes.Industrial_Town: { townType = Any_Town_Phenotype.Industrial_Town; break; }
                        case DormantVolcanoPhenotypes.Mining_Colony: { townType = Any_Town_Phenotype.Mining_Colony; break; }
                        case DormantVolcanoPhenotypes.Mercantile_Trade_Port: { townType = Any_Town_Phenotype.Mercantile_Trade_Port; break; }
                        case DormantVolcanoPhenotypes.Ship_Builders_Collective: { townType = Any_Town_Phenotype.Ship_Builders_Collective; break; }
                        case DormantVolcanoPhenotypes.Stronghold: { townType = Any_Town_Phenotype.Stronghold; break; }
                        case DormantVolcanoPhenotypes.Sailors_Respite: { townType = Any_Town_Phenotype.Sailors_Respite; break; }
                        case DormantVolcanoPhenotypes.Ranchland: { townType = Any_Town_Phenotype.Ranchland; break; }
                        case DormantVolcanoPhenotypes.Wood_Shrouded: { townType = Any_Town_Phenotype.Wood_Shrouded; break; }
                        case DormantVolcanoPhenotypes.Native_Island: { townType = Any_Town_Phenotype.Native_Island; break; }
                    }
                    break;
                }
            case IslandMaster.IslandBiome.ActiveVolcano:
                {
                    setTownTier = Mathf.RoundToInt(Random.Range(0, 4));
                    var activeVolcanoPhenotypes = (ActiveVolcanoPhenotypes)Random.Range(0, System.Enum.GetValues(typeof(ActiveVolcanoPhenotypes)).Length);
                    //CS, FP, FV, LK, IT, MC, St, PC, NI
                    switch (activeVolcanoPhenotypes)
                    {
                        case ActiveVolcanoPhenotypes.Caravan_Site: { townType = Any_Town_Phenotype.Caravan_Site; break; }
                        case ActiveVolcanoPhenotypes.Free_Port: { townType = Any_Town_Phenotype.Free_Port; break; }
                        case ActiveVolcanoPhenotypes.Fishing_Village: { townType = Any_Town_Phenotype.Fishing_Village; break; }
                        case ActiveVolcanoPhenotypes.Lighthouse_Keep: { townType = Any_Town_Phenotype.Lighthouse_Keep; break; }
                        case ActiveVolcanoPhenotypes.Industrial_Town: { townType = Any_Town_Phenotype.Industrial_Town; break; }
                        case ActiveVolcanoPhenotypes.Mining_Colony: { townType = Any_Town_Phenotype.Mining_Colony; break; }
                        case ActiveVolcanoPhenotypes.Stronghold: { townType = Any_Town_Phenotype.Stronghold; break; }
                        case ActiveVolcanoPhenotypes.Penal_Colony: { townType = Any_Town_Phenotype.Sailors_Respite; break; }
                        case ActiveVolcanoPhenotypes.Native_Island: { townType = Any_Town_Phenotype.Native_Island; break; }
                    }
                    break;
                }
            case IslandMaster.IslandBiome.Tundra:
                {
                    setTownTier = Mathf.RoundToInt(Random.Range(0, 3));
                    var tundraPhenotypes = (TundraPhenotypes)Random.Range(0, System.Enum.GetValues(typeof(TundraPhenotypes)).Length);
                    //FP, FV, LK, MTP, St, SR, PC, NI
                    switch (tundraPhenotypes)
                    {
                        case TundraPhenotypes.Free_Port: { townType = Any_Town_Phenotype.Free_Port; break; }
                        case TundraPhenotypes.Fishing_Village: { townType = Any_Town_Phenotype.Fishing_Village; break; }
                        case TundraPhenotypes.Lighthouse_Keep: { townType = Any_Town_Phenotype.Lighthouse_Keep; break; }
                        case TundraPhenotypes.Mercantile_Trade_Port: { townType = Any_Town_Phenotype.Mercantile_Trade_Port; break; }
                        case TundraPhenotypes.Stronghold: { townType = Any_Town_Phenotype.Stronghold; break; }
                        case TundraPhenotypes.Sailors_Respite: { townType = Any_Town_Phenotype.Sailors_Respite; break; }
                        case TundraPhenotypes.Penal_Colony: { townType = Any_Town_Phenotype.Sailors_Respite; break; }
                        case TundraPhenotypes.Native_Island: { townType = Any_Town_Phenotype.Native_Island; break; }
                    }
                    break;
                }
            case IslandMaster.IslandBiome.Ethereal:
                {
                    setTownTier = Mathf.RoundToInt(Random.Range(0, 5));
                    var etherealPhenotypes = (EtherealPhenotypes)Random.Range(0, System.Enum.GetValues(typeof(EtherealPhenotypes)).Length);
                    //CS, FP, FV, MC, LK, MTP, St, PC, WS, NI
                    switch (etherealPhenotypes)
                    {
                        case EtherealPhenotypes.Caravan_Site: { townType = Any_Town_Phenotype.Caravan_Site; break; }
                        case EtherealPhenotypes.Free_Port: { townType = Any_Town_Phenotype.Free_Port; break; }
                        case EtherealPhenotypes.Fishing_Village: { townType = Any_Town_Phenotype.Fishing_Village; break; }
                        case EtherealPhenotypes.Mining_Colony: { townType = Any_Town_Phenotype.Mining_Colony; break; }
                        case EtherealPhenotypes.Lighthouse_Keep: { townType = Any_Town_Phenotype.Lighthouse_Keep; break; }
                        case EtherealPhenotypes.Mercantile_Trade_Port: { townType = Any_Town_Phenotype.Mercantile_Trade_Port; break; }
                        case EtherealPhenotypes.Stronghold: { townType = Any_Town_Phenotype.Stronghold; break; }
                        case EtherealPhenotypes.Penal_Colony: { townType = Any_Town_Phenotype.Sailors_Respite; break; }
                        case EtherealPhenotypes.Wood_Shrouded: { townType = Any_Town_Phenotype.Wood_Shrouded; break; }
                        case EtherealPhenotypes.Native_Island: { townType = Any_Town_Phenotype.Native_Island; break; }
                    }
                    break;
                }
            case IslandMaster.IslandBiome.Deadlands:    //Town will generate dead
                {
                    setTownTier = Mathf.RoundToInt(Random.Range(0, 5));
                    var deadlandsPhenotypes = (DeadlandsPhenotypes)Random.Range(0, System.Enum.GetValues(typeof(DeadlandsPhenotypes)).Length);
                    //FP, FV, LK, MC, MTP, SBC, St, PC, RL, WS, NI
                    switch (deadlandsPhenotypes)
                    {
                        case DeadlandsPhenotypes.Free_Port: { townType = Any_Town_Phenotype.Free_Port; break; }
                        case DeadlandsPhenotypes.Fishing_Village: { townType = Any_Town_Phenotype.Fishing_Village; break; }
                        case DeadlandsPhenotypes.Lighthouse_Keep: { townType = Any_Town_Phenotype.Lighthouse_Keep; break; }
                        case DeadlandsPhenotypes.Mining_Colony: { townType = Any_Town_Phenotype.Mining_Colony; break; }
                        case DeadlandsPhenotypes.Mercantile_Trade_Port: { townType = Any_Town_Phenotype.Mercantile_Trade_Port; break; }
                        case DeadlandsPhenotypes.Ship_Builders_Collective: { townType = Any_Town_Phenotype.Ship_Builders_Collective; break; }
                        case DeadlandsPhenotypes.Stronghold: { townType = Any_Town_Phenotype.Stronghold; break; }
                        case DeadlandsPhenotypes.Penal_Colony: { townType = Any_Town_Phenotype.Sailors_Respite; break; }
                        case DeadlandsPhenotypes.Ranchland: { townType = Any_Town_Phenotype.Ranchland; break; }
                        case DeadlandsPhenotypes.Wood_Shrouded: { townType = Any_Town_Phenotype.Wood_Shrouded; break; }
                        case DeadlandsPhenotypes.Native_Island: { townType = Any_Town_Phenotype.Native_Island; break; }
                    }
                    break;
                }
        }
        townType = (Any_Town_Phenotype)Random.Range(0, System.Enum.GetValues(typeof(Any_Town_Phenotype)).Length);
    }
    void StartTownPlanning()
    {
        associatedTownPlanner.GetPossibleStructures(townType);
        //Debug.Log("Town setup should have triggered with town tier " + setTownTier);
        for (int i = 0; i < setTownTier; i++)
        {
            //Debug.Log("Town setup triggered");
            associatedTownPlanner.SetTownConstructionOrder(i, townType);
            associatedTownPlanner.SetSpecialConstruction(i);
        }
    }
}
