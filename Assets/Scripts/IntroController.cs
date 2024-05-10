using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroController : MonoBehaviour
{
    public TMP_Text[] textElements;
    public float zoomSpeed = 1f;
    public float maxZoom = 2f;
    public float zoomDuration = 5f;
    public float textDuration = 3f;

    public GameObject textObject1;
    public GameObject textObject2;
    public Animator transitionAnimator;

    private int currentTextIndex = 0;

    void Start()
    {
        ActivateTextElements();

        //Invoke("StartNextPhase", zoomDuration);
    }


    void ActivateTextElements()
    {
        foreach (TMP_Text textElement in textElements)
        {
            textElement.enabled = true;
        }
    }

    void StartNextPhase()
    {
        // Enable the circle objects
        foreach (var circle in GetComponentsInChildren<Image>())
        {
            circle.enabled = true;
        }

        // Display text for a short duration
        if (currentTextIndex < textElements.Length)
        {
            textElements[currentTextIndex].enabled = true;
            Invoke("HideText", textDuration);
            currentTextIndex++;
        }
    }

    void HideText()
    {
        // Hide the current text and start the next phase
        if (currentTextIndex - 1 >= 0 && currentTextIndex - 1 < textElements.Length)
        {
            textElements[currentTextIndex - 1].enabled = false;
        }

        // Start the next phase
        Invoke("StartNextPhase", 0.5f); // Adjust delay as needed
    }

    public void ContinueText()
    {
        textObject1.SetActive(false);
        textObject2.SetActive(true);
        Invoke("FadeOut", 7f);
        Invoke("LoadMainLevel", 8f);
    }

    private void FadeOut()
    {
        transitionAnimator.SetTrigger("FadeOut");
    }

    void LoadMainLevel()
    {
        SceneManager.LoadScene("MainScene");
    }
}