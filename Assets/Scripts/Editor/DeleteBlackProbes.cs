using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class DeleteBlackProbes
{
    [MenuItem("Tools/Delete Black Light Probes")]
    public static void ShowWindow()
    {
        CleanBlackProbes();
    }

    private static void CleanBlackProbes()
    {
        LightProbeGroup[] probeGroups = UnityEngine.Object.FindObjectsByType<LightProbeGroup>();
        int totalDeleted = 0;

        foreach (LightProbeGroup group in probeGroups)
        {
            List<Vector3> originalProbes = new List<Vector3>(group.probePositions);
            List<Vector3> validProbes = new List<Vector3>();
            int deletedInGroup = 0;

            for (int i = 0; i < originalProbes.Count; i++)
            {
                Vector3 worldPos = group.transform.TransformPoint(originalProbes[i]);

                UnityEngine.Rendering.SphericalHarmonicsL2 sh;
                LightProbes.GetInterpolatedProbe(worldPos, null, out sh);

                Color averageColor = new Color(sh[0, 0], sh[1, 0], sh[2, 0]);
                if (averageColor is { r: <= 0.15f, g: <= 0.15f, b: <= 0.15f })
                {
                    deletedInGroup++;
                }
                else
                {
                    validProbes.Add(originalProbes[i]);
                }
            }

            if (deletedInGroup > 0)
            {
                Undo.RecordObject(group, "Delete Black Probes");
                group.probePositions = validProbes.ToArray();
                EditorUtility.SetDirty(group);
                totalDeleted += deletedInGroup;
            }
        }

        Debug.Log($"Deleted {totalDeleted} black light probes across the scene");
    }
}
