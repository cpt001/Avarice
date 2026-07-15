using UnityEngine;

public class QuickDetector : MonoBehaviour
{
    private BoxCollider bldgCol => GetComponent<BoxCollider>();


    void Update()
    {
        Collider[] hitColliders = Physics.OverlapBox(transform.position, transform.localScale / 2, Quaternion.identity, ~LayerMask.NameToLayer("TownBuilding"));
        
        foreach (Collider c in hitColliders)
        {
            if (c != bldgCol)
            {
                Debug.Log("Live trigger found: " + c.gameObject);
            }
        }
    }
}
