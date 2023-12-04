using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextUtility : MonoBehaviour
{
    public TextGroup[] textGroups;
    private int currentGroupIndex = 0;

    void Start()
    {
        ActivateTextGroup(currentGroupIndex);
    }

    public void NextGroup()
    {
        // Hide the current group
        DeactivateTextGroup(currentGroupIndex);

        // Move to the next group
        currentGroupIndex++;

        // If there are more groups, activate the next one
        if (currentGroupIndex < textGroups.Length)
        {
            ActivateTextGroup(currentGroupIndex);
        }
        else
        {
            // If there are no more groups, you can perform any action or transition to the next scene.
            Debug.Log("End of text groups");
        }
    }

    void ActivateTextGroup(int groupIndex)
    {
        TextGroup textGroup = textGroups[groupIndex];
        foreach (TMP_Text textElement in textGroup.textElements)
        {
            textElement.gameObject.SetActive(true);
        }
    }

    void DeactivateTextGroup(int groupIndex)
    {
        TextGroup textGroup = textGroups[groupIndex];
        foreach (TMP_Text textElement in textGroup.textElements)
        {
            textElement.gameObject.SetActive(false);
        }
    }

    // Method to handle button click
    public void OnButtonClick()
    {
        NextGroup();
    }
}

[System.Serializable]
public class TextGroup
{
    public TMP_Text[] textElements;
}