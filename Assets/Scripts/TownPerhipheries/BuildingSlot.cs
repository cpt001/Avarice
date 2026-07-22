using UnityEngine;

public class BuildingSlot : MonoBehaviour
{
    public enum SlotFillEnum
    {
        Available,
        Occupied,
        Unavailable,
    }
   public SlotFillEnum slotStatus;

    public void LookForBuilding()
    {
        Collider[] hitColliders = Physics.OverlapBox(transform.position, transform.localScale / 2, Quaternion.identity, 1 << LayerMask.NameToLayer("TownBuilding"));
        foreach (Collider c in hitColliders)
        {
            if (c != GetComponent<BoxCollider>())
            {
                //This breaks the search and set algorithm. Needs better implementation
                Destroy(gameObject);
            }
        }
    }
}
