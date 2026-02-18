using UnityEngine;

/// <summary>
/// This script holds information on individual structures
/// - Type
/// - Construction Resources Required
/// - Number Housed and Workers
/// - Building owner name
/// - Tax upkeep
/// - Resource supply/demand
/// - Player Shopkeep data
/// - Damage
/// </summary>

public class Town_Building : MonoBehaviour
{
    private enum T0Colony
    {
        FishingHut,         // Num workers: 2 || Num Housed: 2 - Shack can house 2 partners + kids, House can do 4
        GypsyWagon,         // 2 || 2
        HunterShack,        // 3 || 2 - 1 Shack
        LoggingCamp,        // 6 || 0 - 3 Shacks
        MarketStall,        // 2 || 0 - 
    }
    private T0Colony t0Structure;
    private enum T1Township
    {
        Apiary,             // 3 || 3
        Bakery,             // 4 || 2 - 1 Shack
        Blacksmith,         // 3 || 2 - 1 Shack
        BawdyHouse,         // 10 || 10
        ClayPit,            // 10 || 0 - 5 Shacks
        Prison,             // 2 || 0
        SawMill,            // 10 || 0
        Tavern,             // 4 || 14 - Has temp houses -7 shacks
    }
    private T1Township t1Structure;
    private enum T2SmallPort
    {
        Apothecary,         // 3 || 3
        Armory,             // 1 || 0 - 1 Shack
        Barn,               // 4 || 0 - 2 Shacks
        Butcher,            // 3 || 3
        Carpenter,          // 4 || 4
        Church,             // 1 || 1
        Cobbler,            // 2 || 2
        Garrison,           // 6 || 6
        Leathersmith,       // 3 || 3
        PawnShop,           // 2 || 2
        Tailor,             // 4 || 4
        TarKiln,            // 3 || 0 - 2 Shacks
        Warehouse,          // 3 || 2 - 1 Shack
        Watchtower,         // 2 || 0 - 1 Shack
    }
    private T2SmallPort t2Structure;
    private enum T3LargePort
    {
        Barber,             // 2 || 2
        Broker,             // 1 || 0 - 1 Shack
        CandleMaker,        // 2 || 2
        JewelerParlor,      // 2 || 2
        Mill,               // 2 || 0 - 1 Shack
        Shipwright,         // 2 || 0 - 1 Shack
        TattooParlor,       // 2 || 2
    }
    private T3LargePort t3Structure;
    private enum T4Capital
    {
        Bank,               // 4 || 0 - 1 House
        Drydock,            // 10 || 0 - 5 Shacks
        House,              // 1 || 4
        Library,            // 3 || 3 
        WigMaker,           // 2 || 2
        Distillery,         // 6 || 0 - 3 Shacks
        Forge,              // 10 || 0 - 5 Shacks
    }
    private T4Capital t4Structure;
    private enum Specialist
    {
        Mineshaft,          // 20 || 10 shacks
        Quarry,             // 20 || 10 shacks
        Plantation,         // 20 || 10 shacks
    }
    private Specialist specialistStructure;
}
