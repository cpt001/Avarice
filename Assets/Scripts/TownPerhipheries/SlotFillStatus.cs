using UnityEngine;

public class SlotFillStatus : MonoBehaviour
{
    public int distanceFromTownCenter;
    public enum SlotFillEnum
    {
        Available,
        Occupied,
        Unavailable,
    }
   public SlotFillEnum slotStatus;
}
