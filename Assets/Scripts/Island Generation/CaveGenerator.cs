using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    //Removed dungeon, added suggested types
    public enum CaveType
    {
        Moist,
        Submerged,
        Volcanic,
        Crypt,
        Mineshaft,
        Rocky,
        Cenote,
        City,
        Temple,

        SeaCave,
        Saltpetre,
        SmugglerCove,
        Ossuary, 
    }
    public CaveType caveType;

    public enum Flags
    {
        Active,
        Abandoned,
        Flooded,
        Collapsed,
        Haunted,
    }
    public Flags flag;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
