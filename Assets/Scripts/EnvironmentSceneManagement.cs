using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnvironmentSceneManagement : MonoBehaviour
{
    static readonly int FadeOut = Animator.StringToHash("FadeOut");

    [Header("Level Configuration")]
    [SerializeField, Tooltip("Enable to ensure the main scene is loaded upon starting the game")] private bool loadMainScene = true;
    [SerializeField, Tooltip("The name of the main scene")] private string mainSceneName = "MainScene";

    public CinemachineCamera introCam;
    public Animator transitionAnimator;

    private void Awake()
    {
        // check if the scene is loaded
        if (!SceneManager.GetSceneByName(mainSceneName).isLoaded && loadMainScene && !Application.isEditor)
        {
            // load the scene on top of the current scene
            SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);
        }
    }

    // workaround for editor
    private IEnumerator Start()
    {
        if (!SceneManager.GetSceneByName(mainSceneName).isLoaded && loadMainScene && Application.isEditor)
        {
            // load the scene on top of the current scene
            SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);
        }

        yield return null;
        yield return null;
        yield return null;

        transitionAnimator.SetTrigger(FadeOut);
        introCam.enabled = false;
    }
}
