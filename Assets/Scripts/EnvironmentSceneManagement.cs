using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnvironmentSceneManagement : MonoBehaviour
{
    [Header("Level Configuration")]
    [SerializeField, Tooltip("Enable to ensure the main scene is loaded upon starting the game")] private bool loadMainScene = true;
    [SerializeField, Tooltip("The name of the main scene")] private string mainSceneName = "MainScene";

    private void Awake()
    {
        // check if the scene is loaded
        if (SceneManager.GetSceneByName(mainSceneName).isLoaded == false && loadMainScene && !Application.isEditor)
        {
            // load the scene on top of the current scene
            SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);
        }
    }

    // workaround for editor
    private void Start()
    {
        if (SceneManager.GetSceneByName(mainSceneName).isLoaded == false && loadMainScene && Application.isEditor)
        {
            // load the scene on top of the current scene
            SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);
        }
    }
}
