using UnityEngine;

/// <summary>
/// This script holds information on individual structures
/// - Name
/// - Ownership details and updated tax rate
/// - Associated data not kept in SO
/// - Model used based on Biome
/// - Damage state
/// 
/// Maybe define building size 1-4, then have this building change into the appropriate model
/// </summary>

[RequireComponent(typeof(BuildingSpawnChanceData))]
public class Town_Building : MonoBehaviour
{
    public Building buildingData;
    private BuildingSpawnChanceData spawnChanceData => GetComponent<BuildingSpawnChanceData>();
    [SerializeField] private GameObject buildingModel;
    private enum DamageState
    {
        Nominal,            //The building is fine
        Worn,               //Building is worn down in places, and could use some minor repair
        Ramshackle,         //Building is mildly damaged, and may even have holes present
        SevereDamage,       //Building is seriously damaged
        Unstable,           //Building may collapse at any moment
        Rubble,             //Building has collapsed
    }
    [SerializeField] private DamageState damage;
    public int BuildingTier;
    public bool isSpecialStructure;
    public bool isUnderConstruction;
}
//FTP accesses master list, and populates phenotypes with matching tags
