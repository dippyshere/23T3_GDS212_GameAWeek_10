#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LODLightmapBakeUtility
{
    private const string MenuRoot = "Tools/LOD Lightmap Bake/";

    [MenuItem(MenuRoot + "Prepare For Bake")]
    public static void PrepareForBake()
    {
        int groupsTouched = 0;
        int renderersChanged = 0;

        foreach (var scene in EnumerateLoadedScenes())
        {
            bool sceneDirty = false;

            foreach (var group in FindLODGroups(scene))
            {
                var lods = group.GetLODs();
                if (lods == null || lods.Length < 2)
                    continue;

                if (lods[0].renderers == null || lods[0].renderers.Length == 0)
                    continue;

                if (!HasAnyRenderableLightmappedLOD0(lods[0].renderers))
                    continue;

                bool groupChanged = false;

                for (int lodIndex = 1; lodIndex < lods.Length; lodIndex++)
                {
                    var renderers = lods[lodIndex].renderers;
                    if (renderers == null)
                        continue;

                    foreach (var renderer in renderers)
                    {
                        if (renderer == null)
                            continue;

                        if (SetScaleInLightmap(renderer, 0f))
                        {
                            renderersChanged++;
                            groupChanged = true;
                        }
                    }
                }

                if (groupChanged)
                {
                    groupsTouched++;
                    sceneDirty = true;
                }
            }

            if (sceneDirty)
                EditorSceneManager.MarkSceneDirty(scene);
        }

        Debug.Log($"LOD Lightmap Bake: prepared {groupsTouched} LOD groups and set {renderersChanged} renderers to Scale In Lightmap = 0");
    }

    [MenuItem(MenuRoot + "Restore After Bake")]
    public static void RestoreAfterBake()
    {
        int groupsTouched = 0;
        int renderersChanged = 0;

        foreach (var scene in EnumerateLoadedScenes())
        {
            bool sceneDirty = false;

            foreach (var group in FindLODGroups(scene))
            {
                var lods = group.GetLODs();
                if (lods == null || lods.Length == 0)
                    continue;

                var lod0Renderers = lods[0].renderers;
                var lod0Source = FindFirstValidRenderer(lod0Renderers);
                if (lod0Source == null)
                    continue;

                float lod0Scale = GetScaleInLightmap(lod0Source);
                int lod0LightmapIndex = lod0Source.lightmapIndex;
                Vector4 lod0ScaleOffset = lod0Source.lightmapScaleOffset;

                bool groupChanged = false;

                for (int lodIndex = 1; lodIndex < lods.Length; lodIndex++)
                {
                    var targetRenderers = lods[lodIndex].renderers;
                    if (targetRenderers == null)
                        continue;

                    for (int i = 0; i < targetRenderers.Length; i++)
                    {
                        var target = targetRenderers[i];
                        if (target == null)
                            continue;

                        var source = (lod0Renderers != null && i < lod0Renderers.Length && lod0Renderers[i] != null)
                            ? lod0Renderers[i]
                            : lod0Source;

                        bool changed = false;

                        float sourceScale = GetScaleInLightmap(source);
                        if (SetScaleInLightmap(target, sourceScale))
                            changed = true;

                        if (target.lightmapIndex != source.lightmapIndex)
                        {
                            target.lightmapIndex = source.lightmapIndex;
                            changed = true;
                        }

                        if (target.lightmapScaleOffset != source.lightmapScaleOffset)
                        {
                            target.lightmapScaleOffset = source.lightmapScaleOffset;
                            changed = true;
                        }

                        if (changed)
                        {
                            renderersChanged++;
                            groupChanged = true;
                            EditorUtility.SetDirty(target);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                        }
                    }
                }

                if (groupChanged)
                {
                    groupsTouched++;
                    sceneDirty = true;
                }
            }

            if (sceneDirty)
                EditorSceneManager.MarkSceneDirty(scene);
        }

        Debug.Log($"LOD Lightmap Bake: restored {groupsTouched} LOD groups and updated {renderersChanged} renderers to match LOD0 lightmap data");
    }

    [MenuItem(MenuRoot + "Bake Lighting With LOD Prep")]
    public static void BakeLightingWithPrep()
    {
        PrepareForBake();
        Lightmapping.Bake();
        RestoreAfterBake();
    }

    private static IEnumerable<Scene> EnumerateLoadedScenes()
    {
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            if (scene.IsValid() && scene.isLoaded)
                yield return scene;
        }
    }

    private static IEnumerable<LODGroup> FindLODGroups(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root == null)
                continue;

            foreach (var group in root.GetComponentsInChildren<LODGroup>(true))
            {
                if (group != null)
                    yield return group;
            }
        }
    }

    private static bool HasAnyRenderableLightmappedLOD0(Renderer[] renderers)
    {
        foreach (var renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (renderer.enabled && renderer.gameObject.activeInHierarchy && GetScaleInLightmap(renderer) > 0f)
                return true;
        }

        return false;
    }

    private static Renderer FindFirstValidRenderer(Renderer[] renderers)
    {
        if (renderers == null)
            return null;

        foreach (var renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (renderer.enabled && renderer.gameObject.activeInHierarchy)
                return renderer;
        }

        foreach (var renderer in renderers)
        {
            if (renderer != null)
                return renderer;
        }

        return null;
    }

    private static float GetScaleInLightmap(Renderer renderer)
    {
        var so = new SerializedObject(renderer);
        var prop = so.FindProperty("m_ScaleInLightmap");
        return prop != null ? prop.floatValue : 1f;
    }

    private static bool SetScaleInLightmap(Renderer renderer, float value)
    {
        var so = new SerializedObject(renderer);
        var prop = so.FindProperty("m_ScaleInLightmap");
        if (prop == null)
            return false;

        if (Mathf.Approximately(prop.floatValue, value))
            return false;

        prop.floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(renderer);
        PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        return true;
    }
}
#endif
