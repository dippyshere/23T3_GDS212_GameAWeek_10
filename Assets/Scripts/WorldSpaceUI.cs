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

    private CanvasGroup canvasGroup;

    private void Start()
    {
        // Assuming the player has a "Player" tag, you can change this as needed.
        player = GameObject.FindGameObjectWithTag("Player").transform;
        canvasGroup = GetComponent<CanvasGroup>();
        SetVisibility(false);
    }

    private void Update()
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
        if (canvasGroup != null)
        {
            canvasGroup.alpha = isVisible ? 1f : 0f;
            canvasGroup.interactable = isVisible;
            canvasGroup.blocksRaycasts = isVisible;
        }
        else
        {
            Debug.LogError("CanvasGroup component not found. Add a CanvasGroup to your canvas.");
        }

        // Set the visibility of the Text element if a reference is provided.
        if (textElement != null)
        {
            textElement.enabled = isVisible;
        }
    }
}