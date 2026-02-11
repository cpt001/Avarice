using UnityEngine;

public class Town_Naming : MonoBehaviour
{
    private enum PrefixList
    {
        Run,
        Gold,
        Sea,
        Rum,
        Kil,
        Mer,
        Mermaid,
        New,
        Dew,
        Little,
        Ship,
        Cape,
        Sail,
        Wreck,
        Siren,
        No,
        North,
        East,
        West,
        South,
        Market,
        Mark,
        Maker,
    }
    private PrefixList townPrefix;
    private enum SuffixList
    {
        ville,
        town,
        port,
        berg,
        borne,
        forth,
        point,
        folk,
        fort,
        stead,
        worth,
        pool,
        well,
        wick,
        bury,
        stone,
        ington,
        ingville,
        ship,
        state,
        lake,
        plain,
        bar,
        ridge,
        blight,
        flats,
        castle,
        bay,
    }
    private SuffixList townSuffix;

    private string givenName;
    public string NameOutput()
    {
        int usesFounderName = Random.Range(0, 2);
        if (usesFounderName == 1)
        {
            givenName = "FounderNameNYI";
        }
        else
        {
            townPrefix = (PrefixList)Random.Range(0, System.Enum.GetValues(typeof(PrefixList)).Length);
            givenName = townPrefix.ToString();
        }

        int hasSuffix = Random.Range(0, 2);
        if (hasSuffix == 1)
        {
            townSuffix = (SuffixList)Random.Range(0, System.Enum.GetValues(typeof(SuffixList)).Length);
            givenName = townPrefix.ToString() + townSuffix.ToString();
        }
        return givenName;
    }
}
