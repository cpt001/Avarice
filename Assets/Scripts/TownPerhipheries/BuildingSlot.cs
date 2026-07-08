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
}
