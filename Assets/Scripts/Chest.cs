using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeightedGem
{
    public GameObject gemPrefab;
    public float weight;
}

public class Chest : MonoBehaviour
{
    [SerializeField]
    private WeightedGem[] _weightedGemPrefabs; // Array of weighted gem prefabs

    // Make weightedGemPrefabs public
    public WeightedGem[] weightedGemPrefabs
    {
        get { return _weightedGemPrefabs; }
        set { _weightedGemPrefabs = value; }
    }

    [SerializeField]
    private GameObject lidObject; // Reference to the lid object

    private bool isOpen = false;

    // Called when the player interacts with the chest
    public void OpenChest()
    {
        if (isOpen)
        {
            return;
        }
        // Calculate the total weight of all gems
        //float totalWeight = 0f;
        //foreach (WeightedGem weightedGem in _weightedGemPrefabs)
        //{
        //    totalWeight += weightedGem.weight;
        //}

        // Choose a random value within the total weight range
        //float randomValue = Random.Range(0f, totalWeight);

        // Iterate through the gem prefabs and find the one corresponding to the chosen value
        //foreach (WeightedGem weightedGem in _weightedGemPrefabs)
        //{
        //    randomValue -= weightedGem.weight;
        //    if (randomValue <= 0f)
        //    {
        // Spawn the chosen gemPrefab at the chest's position
        //Instantiate(weightedGem.gemPrefab, transform.position, Quaternion.identity);

        // Deactivate the lid object
        if (lidObject != null)
        {
            lidObject.SetActive(false);
        }

        // Mark the chest as open
        isOpen = true;

        // Optionally, you can disable the entire chest or play an opening animation
        // For example: gameObject.SetActive(false);

        // Exit the loop once a gem is selected
        //        break;
        //    }
        //}
        // temp code until interactable gems are added
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject t in gameObjects)
        {
            if (t.GetComponent<PlayerController>())
            {
                GameObject player = t;
                int randomGemType = Random.Range(0, 3);
                switch (randomGemType)
                {
                    case 0:
                        player.GetComponent<PlayerController>().AddGems(PlayerController.GemTradeType.Stone1);
                        break;
                    case 1:
                        player.GetComponent<PlayerController>().AddGems(PlayerController.GemTradeType.Stone2);
                        break;
                    case 2:
                        player.GetComponent<PlayerController>().AddGems(PlayerController.GemTradeType.Stone5);
                        break;
                    default:
                        player.GetComponent<PlayerController>().AddGems(PlayerController.GemTradeType.Stone6);
                        break;
                }
            }
        }
    }
}