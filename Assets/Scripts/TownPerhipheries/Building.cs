using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuildingScriptableObject", menuName = "ScriptableObjects/Town")]
public class Building : ScriptableObject
{
    public enum BuildingType
    {
        //T0
        FishingHut,
        GypsyWagon,
        HunterShack,
        LoggingCamp,
        MarketStall,

        //T1
        Apiary,
        Bakery,
        BawdyHouse,
        Blacksmith,
        SawMill,
        Tavern,

        //T2
        Apothecary,
        Armory,
        Barn,        
        Carpenter,
        Church,
        Cobbler,
        Garrison,
        Leathersmith,
        PawnShop,
        Tailor,
        TarKiln,
        Warehouse,
        Watchtower,
        
        //T3
        Barber,
        Broker,
        Butcher, 
        CandleMaker,
        JewelerParlor,
        Mill,
        Shipwright,
        TattooParlor,

        //T4
        AdmiraltyHouse,
        Bank,
        Drydock,
        Library,
        WigMaker,
        
        //Capital Only
        Distillery,
        Forge,

        //Specialist
        ClayPit,
        Mineshaft,
        Plantation,
        Quarry,

        //Advancement
        Dock,
        Fort,
        Graveyard,
        TownSquare,
        TownHall,
        Lighthouse,
        GovernorsMansion,
        Clocktower,
        Prison,
        Housing,
        WaterWell,
    }
    [Header("Structure Details")]
    public BuildingType buildingType;
    [SerializeField] private string[] REPLACE_CargoInput;
    [SerializeField] private string[] REPLACE_CargoOutput;

    [Header("Ownership Details")]
    [SerializeField] private int baseTaxRate;
    [SerializeField] private string[] REPLACE_masterShopInventory;
    public int residentMax;
    public int workerMax;

    [Header("Construction Materials")]
    [SerializeField] private int bricksRequired;
    [SerializeField] private int planksRequired;
    [SerializeField] private int toolsRequired;
    public enum BuildingTier
    {
        T1,
        T2,
        T3,
        T4,
        Capital,
        Specialist,
        Advancement
    }
    public BuildingTier buildingTier;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] GenericModelSet;
    [SerializeField] private GameObject[] DesertModelSet;
    [SerializeField] private GameObject[] SwampModelSet;
    [SerializeField] private GameObject[] JungleModelSet;
    [SerializeField] private GameObject[] DormantVolcanoModelSet;
    [SerializeField] private GameObject[] ActiveVolcanicModelSet;
    [SerializeField] private GameObject[] TundraModelSet;
    [SerializeField] private GameObject[] EtherealModelSet;
    //Deadlands skipped. Solution - generate from random existing set, then spawn semi-broken
}
