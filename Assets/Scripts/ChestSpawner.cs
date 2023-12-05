using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    public GameObject chestPrefab; // Reference to the prefab you want to spawn
    public Transform[] spawnerLocations; // Array of spawner locations

    void Start()
    {
        // Call the function to spawn chests at spawner locations
        SpawnChests();
    }

    void SpawnChests()
    {
        // Iterate through each spawner location
        foreach (Transform spawnerLocation in spawnerLocations)
        {
            // Spawn the chest prefab at the spawner location
            Instantiate(chestPrefab, spawnerLocation.position, spawnerLocation.rotation);
        }
    }
}