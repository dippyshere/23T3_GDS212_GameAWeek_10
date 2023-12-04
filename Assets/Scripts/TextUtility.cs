using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextUtility : MonoBehaviour
{
    public Text[] textElements;
    public float textDuration = 3f; // Adjust as needed
    private int textIndex = 0;

    private void Start()
    {
        for (int i = 0; i < textElements.Length; i++)
        {
            HideText(i);
        }

        StartCoroutine(ShowTextWithDelay(textIndex));
    }

    private IEnumerator ShowTextWithDelay(int index)
    {
        yield return new WaitForSeconds(textDuration);

        HideText(index);
        textIndex++;

        if (textIndex < textElements.Length)
        {
            StartCoroutine(ShowTextWithDelay(textIndex));
        }
    }

    private void ShowText(int index)
    {
        if (index >= 0 && index < textElements.Length)
        {
            textElements[index].enabled = true;
        }
    }

    private void HideText(int index)
    {
        if (index >= 0 && index < textElements.Length)
        {
            textElements[index].enabled = false;
        }
    }
}