using UnityEngine;

/// <summary>
/// This script holds information on individual structures
/// - Name
/// - Ownership details and updated tax rate
/// - Associated data not kept in SO
/// - Model used based on Biome
/// - Damage state
/// </summary>

[RequireComponent(typeof(BuildingSpawnChanceData))]
public class Town_Building : MonoBehaviour
{
    public Building buildingData;
    private BuildingSpawnChanceData spawnChanceData => GetComponent<BuildingSpawnChanceData>();
    [SerializeField] private GameObject buildingModel;
    public enum DamageState
    {
        Nominal,            //The building is fine
        Worn,               //Building is worn down in places, and could use some minor repair
        Ramshackle,         //Building is mildly damaged, and may even have holes present
        SevereDamage,       //Building is seriously damaged
        Unstable,           //Building may collapse at any moment
        Rubble,             //Building has collapsed
    }
    public DamageState damage;
    public int BuildingTier;
    public bool isSpecialStructure;
    public bool isUnderConstruction;
    public enum SetupCondition
    {
        Beach,
        CityScape,
        Standalone,
    }
    public SetupCondition setupCondition;
}
//FTP accesses master list, and populates phenotypes with matching tags
