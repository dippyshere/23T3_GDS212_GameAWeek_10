using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    public GameObject chestPrefab; // Reference to the chest prefab
    public Transform[] spawnerLocations; // Array of spawner locations
    public WeightedGem[] weightedGemPrefabs; // Array of weighted gem prefabs

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
            GameObject chest = Instantiate(chestPrefab, spawnerLocation.position, spawnerLocation.rotation);

            // Attach a Chest script to the spawned chest
            Chest chestScript = chest.AddComponent<Chest>();

            // Pass the weightedGemPrefabs array to the Chest script
            chestScript.weightedGemPrefabs = weightedGemPrefabs;
        }
    }
}