using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public static class LODLightmapSync
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void RuntimeInit()
    {
        ApplyToLoadedScenes();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToScene(scene);
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void EditorInit()
    {
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorSceneManager.sceneOpened += OnSceneOpened;

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        // Apply after scripts reload
        EditorApplication.delayCall += ApplyToLoadedScenes;
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        EditorApplication.delayCall += () => ApplyToScene(scene);
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode ||
            state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.delayCall += ApplyToLoadedScenes;
        }
    }

    [MenuItem("Tools/LOD Lightmaps/Apply Now")]
    static void ApplyNow()
    {
        ApplyToLoadedScenes();
        Debug.Log("LOD lightmaps synchronized.");
    }

#endif

    static void ApplyToLoadedScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);

            if (scene.isLoaded)
                ApplyToScene(scene);
        }
    }

    static void ApplyToScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        foreach (var root in scene.GetRootGameObjects())
        {
            var groups = root.GetComponentsInChildren<LODGroup>(true);

            foreach (var group in groups)
                Apply(group);
        }
    }

    static void Apply(LODGroup group)
    {
        var lods = group.GetLODs();

        if (lods.Length < 2)
            return;

        var lod0 = lods[0].renderers;

        if (lod0 == null || lod0.Length == 0)
            return;

        Renderer fallback = null;

        foreach (var r in lod0)
        {
            if (r != null)
            {
                fallback = r;
                break;
            }
        }

        if (fallback == null)
            return;

        for (int lod = 1; lod < lods.Length; lod++)
        {
            var renderers = lods[lod].renderers;

            for (int i = 0; i < renderers.Length; i++)
            {
                var dst = renderers[i];

                if (dst == null)
                    continue;

                Renderer src =
                    (i < lod0.Length && lod0[i] != null)
                    ? lod0[i]
                    : fallback;

                dst.lightmapIndex = src.lightmapIndex;
                dst.lightmapScaleOffset = src.lightmapScaleOffset;
            }
        }
    }
}