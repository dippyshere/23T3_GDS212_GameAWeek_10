using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class IntroTextPrinter : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float textSpeed = 0.05f;
    public float startDelay = 1f;
    public float fadeoutDelay = 1f;
    public float fadeoutDuration = 1f;

    string fullText;

    IEnumerator Start()
    {
        fullText = textComponent.text;
        textComponent.text = "";
        yield return new WaitForSeconds(startDelay);

        foreach (char letter in fullText.ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        yield return new WaitForSeconds(fadeoutDelay);
        StartCoroutine(FadeOutText());
    }

    IEnumerator FadeOutText()
    {
        float elapsedTime = 0f;
        Color originalColor = textComponent.color;

        while (elapsedTime < fadeoutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeoutDuration);
            textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        textComponent.gameObject.SetActive(false);
    }
}
