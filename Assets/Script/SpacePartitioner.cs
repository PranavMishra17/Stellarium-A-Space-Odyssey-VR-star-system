using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpacePartitioner : MonoBehaviour
{
    public Vector3 sectorSize = new Vector3(100, 100, 100);
    public GameObject player; // Assign the player GameObject in the Inspector
    private Dictionary<Vector3Int, StarSector> sectors = new Dictionary<Vector3Int, StarSector>();

    // This overlap value will be used to adjust the sectorSize for overlap effect
    public float overlap = 20f;

    public class StarSector
    {
        public HashSet<GameObject> stars = new HashSet<GameObject>();
    }

    void Start()
    {
        StartCoroutine(DelayedPartitioning());
    }

    IEnumerator DelayedPartitioning()
    {
        yield return new WaitForSeconds(1); // Wait a bit to ensure all stars are loaded
        PartitionStarsIntoSectors();
        Debug.Log($"Partitioned into {sectors.Count} sectors with overlap.");
    }

    void Update()
    {
        UpdateSectorVisibility();
    }

    Vector3Int GetSectorIndex(Vector3 position)
    {
        // Adjust position based on overlap to create naturally overlapping sectors
        return new Vector3Int(
            Mathf.FloorToInt((position.x + overlap) / (sectorSize.x + overlap)),
            Mathf.FloorToInt((position.y + overlap) / (sectorSize.y + overlap)),
            Mathf.FloorToInt((position.z + overlap) / (sectorSize.z + overlap))
        );
    }

    void PartitionStarsIntoSectors()
    {
        GameObject[] stars = GameObject.FindGameObjectsWithTag("Star");
        foreach (var star in stars)
        {
            Vector3Int sectorIndex = GetSectorIndex(star.transform.position);
            if (!sectors.TryGetValue(sectorIndex, out StarSector sector))
            {
                sector = new StarSector();
                sectors[sectorIndex] = sector;
            }
            sector.stars.Add(star);
            star.SetActive(false); // Initially deactivate all stars
        }
    }

    void UpdateSectorVisibility()
    {
        // Deactivate all stars first
        foreach (var sector in sectors.Values)
        {
            foreach (var star in sector.stars)
            {
                star.SetActive(false);
            }
        }

        // Reactivate stars in sectors close to the player
        Vector3Int playerSectorIndex = GetSectorIndex(player.transform.position);
        if (sectors.TryGetValue(playerSectorIndex, out StarSector playerSector))
        {
            foreach (var star in playerSector.stars)
            {
                star.SetActive(true);
            }
            //Debug.Log($"Activated stars in player's sector: {playerSectorIndex}");
        }
    }
}
