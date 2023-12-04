using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroController : MonoBehaviour
{
    public float zoomSpeed = 1f;
    public float maxZoom = 2f;
    public float zoomDuration = 5f; // Adjust as needed

    public Text[] textElements;
    public float textDuration = 3f; // Adjust as needed

    private Camera mainCamera;
    private RectTransform imageTransform;
    private int currentTextIndex = 0;

    void Start()
    {
        mainCamera = Camera.main;
        imageTransform = GetComponentInChildren<Image>().rectTransform;

        // Set initial camera field of view
        mainCamera.fieldOfView = 60f;

        Invoke("StartNextPhase", zoomDuration);
    }

    void Update()
    {
        // Zoom in gradually
        if (Time.time < zoomDuration)
        {
            float newFieldOfView = mainCamera.fieldOfView - zoomSpeed * Time.deltaTime;

            // Clamp zoom to a maximum value
            mainCamera.fieldOfView = Mathf.Clamp(newFieldOfView, maxZoom, 60f);
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
}