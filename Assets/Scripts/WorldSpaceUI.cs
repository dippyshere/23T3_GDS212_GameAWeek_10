using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldSpaceUI : MonoBehaviour
{
    public float activationDistance = 5f;
    private Transform player;

    public TextMeshProUGUI textElement; // Reference to the Text element on the canvas.

    private void Start()
    {
        // Assuming the player has a "Player" tag, you can change this as needed.
        player = GameObject.FindGameObjectWithTag("Player").transform;
        SetVisibility(false);
    }

    private void FixedUpdate()
    {
        // Check the distance between the UI and the player.
        float distance = Vector3.Distance(transform.position, player.position);

        // Adjust UI visibility based on distance.
        if (distance <= activationDistance)
        {
            SetVisibility(true);
        }
        else
        {
            SetVisibility(false);
        }
    }

    private void SetVisibility(bool isVisible)
    {
        // Set the visibility of the Text element if a reference is provided.
        if (textElement != null)
        {
            textElement.enabled = isVisible;
        }
    }
}