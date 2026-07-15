using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SlotBuilder : MonoBehaviour
{
    public GameObject initialSlot;
    public int distanceFromCenter;
    [SerializeField] private GameObject slotPrefab;
    private BoxCollider bldgCol => GetComponent<BoxCollider>();
    private float roadOffset = 0.5f;
    private Vector3 adjustedCollider;
    public List<GameObject> slots = new List<GameObject>();
    private GameObject neCorner;
    private GameObject swCorner;
    private int nodeThreshhold = 3;
    private int numNodesSpawnOnX;
    private int numNodesSpawnOnY;
    private int numSides;

    public bool triggerDetectingObject = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void BuildStructureSlots(bool overrideSideValue)
    {
        //if (GetComponentInParent<Town_Building>().setupCondition == Town_Building.SetupCondition.CityScape)
        {
            adjustedCollider = new Vector3(bldgCol.size.x + roadOffset, 0, bldgCol.size.z + roadOffset);
            numNodesSpawnOnX = Mathf.RoundToInt(adjustedCollider.x * 2 / nodeThreshhold) + 1;
            numNodesSpawnOnY = Mathf.RoundToInt(adjustedCollider.z * 2 / nodeThreshhold);

            SpawnCorners(overrideSideValue); //Also spawns sides
            PickPrimaryHook();
        }
    }

    void SpawnCorners(bool overrideSideValue)
    {
        if (neCorner == null)
        {
            neCorner = Instantiate(slotPrefab, transform.position, transform.rotation, transform);
            neCorner.transform.localPosition += new Vector3(adjustedCollider.x, 0, adjustedCollider.z);
            neCorner.name = "NE";
            slots.Add(neCorner);
        }
        if (swCorner == null)
        {
            swCorner = Instantiate(slotPrefab, transform.position, transform.rotation, transform);
            swCorner.transform.localPosition += new Vector3(-adjustedCollider.x, 0, -adjustedCollider.z);
            swCorner.name = "SW";
        }

        if (!overrideSideValue)
        {
            numSides = Random.Range(1, 5);
        }
        else
        {
            numSides = 4;
        }

        //Debug.Log("Num Sides: " + numSides, gameObject);
        for (int i = 0; i < numSides; i++)
        {
            if (i == 0)
            {
                SpawnEdge(numNodesSpawnOnX, "N", neCorner);
                if (numSides == 1)
                {
                    swCorner.SetActive(false);
                }
            }
            if (i == 1)
            {
                if (numSides == 2)
                {
                    int rand = Random.Range(0, 2);
                    if (rand != 0)
                    {
                        SpawnEdge(numNodesSpawnOnY, "E", neCorner);
                    }
                    else
                    {
                        SpawnEdge(numNodesSpawnOnY, "W", swCorner);
                    }
                    swCorner.SetActive(false);
                }
                else
                {
                    SpawnEdge(numNodesSpawnOnY, "E", neCorner);
                }
            }
            if (i == 2)
            {
                SpawnEdge(numNodesSpawnOnY, "W", swCorner);
                if (numSides == 3)
                {
                    swCorner.SetActive(false);
                }
            }
            if (i == 3)
            {
                SpawnEdge(numNodesSpawnOnX, "S", swCorner);
                slots.Add(swCorner);
            }

        }


    }

    void SpawnEdge(int spawnCount, string direction, GameObject origin)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            if (i != 0)
            {
                GameObject g = Instantiate(slotPrefab, origin.transform.position, transform.rotation, transform);
                g.name = direction;

                if (direction == "N") { g.transform.localPosition += new Vector3(-i * nodeThreshhold, 0); }
                if (direction == "E") { g.transform.localPosition += new Vector3(0, 0, -i * nodeThreshhold); }
                if (direction == "W") { g.transform.localPosition += new Vector3(0, 0, i * nodeThreshhold); }
                if (direction == "S") { g.transform.localPosition += new Vector3(i * nodeThreshhold, 0); }

                slots.Add(g);
            }
        }
    }

    void PickPrimaryHook()
    {
        int rand = Random.Range(0, slots.Count);
        initialSlot = slots[rand];
    }

    public void CheckCollider()
    {
        Collider[] hitColliders = Physics.OverlapBox(transform.position, transform.localScale / 2, Quaternion.identity, ~LayerMask.NameToLayer("TownBuilding"));

        foreach (Collider c in hitColliders)
        {
            if (c != bldgCol)
            {
                //Debug.Log("Live trigger found: " + c.gameObject);
                triggerDetectingObject = true;
            }
        }
    }
}
