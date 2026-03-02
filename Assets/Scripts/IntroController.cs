using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroController : MonoBehaviour
{
    static readonly int Out = Animator.StringToHash("FadeOut");
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
        if (Application.isMobilePlatform)
        {
            Application.targetFrameRate = Mathf.CeilToInt((float)Screen.currentResolution.refreshRateRatio.value);
        }
        else
        {
            Application.targetFrameRate = -1;
        }

        InputSystem.settings.SetInternalFeatureFlag("USE_OPTIMIZED_CONTROLS", true);
        InputSystem.settings.SetInternalFeatureFlag("USE_READ_VALUE_CACHING", true);
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
        foreach (Image circle in GetComponentsInChildren<Image>())
        {
            circle.enabled = true;
        }

        // Display text for a short duration
        if (currentTextIndex >= textElements.Length)
        {
            return;
        }

        textElements[currentTextIndex].enabled = true;
        Invoke(nameof(HideText), textDuration);
        currentTextIndex++;
    }

    void HideText()
    {
        // Hide the current text and start the next phase
        if (currentTextIndex - 1 >= 0 && currentTextIndex - 1 < textElements.Length)
        {
            textElements[currentTextIndex - 1].enabled = false;
        }

        // Start the next phase
        Invoke(nameof(StartNextPhase), 0.5f); // Adjust delay as needed
    }

    public void ContinueText()
    {
        textObject1.SetActive(false);
        textObject2.SetActive(true);
        Invoke(nameof(FadeOut), 7f);
        Invoke(nameof(LoadMainLevel), 9f);
    }

    private void FadeOut()
    {
        transitionAnimator.SetTrigger(Out);
    }

    void LoadMainLevel()
    {
        SceneManager.LoadScene("MainScene");
    }
}