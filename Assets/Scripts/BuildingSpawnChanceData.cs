using UnityEngine;
using System.Collections.Generic;

public class BuildingSpawnChanceData : MonoBehaviour
{
    [SerializeField] private int Caravan_Site_Chance;
    [SerializeField] private int Fertile_Island_Chance;
    [SerializeField] private int Free_Port_Chance;
    [SerializeField] private int Fishing_Village_Chance;
    [SerializeField] private int Lighthouse_Keep_Chance;
    [SerializeField] private int Industrial_Town_Chance;
    [SerializeField] private int Mining_Colony_Chance;
    [SerializeField] private int Mercantile_Trade_Port_Chance;
    [SerializeField] private int Ship_Builders_Collective_Chance;
    [SerializeField] private int Stronghold_Chance;
    [SerializeField] private int Sailors_Respite_Chance;
    [SerializeField] private int Swamp_Town_Chance;
    [SerializeField] private int Penal_Colony_Chance;
    [SerializeField] private int Ranchland_Chance;
    [SerializeField] private int Wood_Shrouded_Chance;
    [SerializeField] private int Native_Island_Chance;

    public Dictionary<Any_Town_Phenotype, int> phenotypeChanceDict = new Dictionary<Any_Town_Phenotype, int>();

    private void Awake()
    {
        phenotypeChanceDict.Add(Any_Town_Phenotype.Caravan_Site, Caravan_Site_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Fertile_Island, Fertile_Island_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Free_Port, Free_Port_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Fishing_Village, Fishing_Village_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Lighthouse_Keep, Lighthouse_Keep_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Industrial_Town, Industrial_Town_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Mining_Colony, Mining_Colony_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Mercantile_Trade_Port, Mercantile_Trade_Port_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Ship_Builders_Collective, Ship_Builders_Collective_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Stronghold, Stronghold_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Sailors_Respite, Sailors_Respite_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Swamp_Town, Swamp_Town_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Penal_Colony, Penal_Colony_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Ranchland, Ranchland_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Wood_Shrouded, Wood_Shrouded_Chance);
        phenotypeChanceDict.Add(Any_Town_Phenotype.Native_Island, Native_Island_Chance);
    }
}
