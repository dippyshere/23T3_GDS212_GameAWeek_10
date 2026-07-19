using System.Collections;
using TMPro;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroController : MonoBehaviour
{
    [NoAutoStaticsCleanup]
    static readonly int Out = Animator.StringToHash("FadeOut");
    [NoAutoStaticsCleanup]
    static readonly int In = Animator.StringToHash("FadeIn");
    public Animator transitionAnimator;
    public bool isEnd = false;

    void Start()
    {
        if (isEnd)
        {
            transitionAnimator.SetTrigger(In);
            return;
        }
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

        Invoke(nameof(FadeOut), 22f);
        Invoke(nameof(LoadMainLevel), 24f);
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