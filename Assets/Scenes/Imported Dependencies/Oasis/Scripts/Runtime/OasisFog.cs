using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OasisFog : FullscreenEffectBase<OasisFogPass>
{
    public override void OnBeginCamera(ScriptableRenderContext ctx, Camera cam)
    {
        base.OnBeginCamera(ctx, cam);
    }
}

public class OasisFogPass : FullscreenPassBase<FullscreenPassDataBase>
{
    static readonly int Tint = Shader.PropertyToID("_Tint");
    static readonly int Density = Shader.PropertyToID("_Density");
    static readonly int StartDistance = Shader.PropertyToID("_StartDistance");
    static readonly int SunScatteringIntensity = Shader.PropertyToID("_SunScatteringIntensity");
    static readonly int HeightRange = Shader.PropertyToID("_Height_Range");

    void UpdateVolumeSettings()
    {
        OasisFogVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<OasisFogVolumeComponent>();

        float fogDensity = volumeComponent.Density.value;
        if (fogDensity < Mathf.Epsilon) return;

        float fogStartDistance = volumeComponent.StartDistance.value;
        Color fogTint = volumeComponent.Tint.value;
        float fogSunScatteringIntensity = volumeComponent.SunScatteringIntensity.value;
        Vector2 fogHeightRange = volumeComponent.HeightRange.value;

        material.SetColor(Tint, fogTint);
        material.SetFloat(Density, fogDensity);
        material.SetFloat(StartDistance, fogStartDistance);
        material.SetFloat(SunScatteringIntensity, fogSunScatteringIntensity);
        material.SetVector(HeightRange, fogHeightRange);
    }

#if !UNITY_6000_4_OR_NEWER 
#pragma warning disable CS0618
#pragma warning disable CS0672
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        UpdateVolumeSettings();

        base.Execute(context, ref renderingData);
    }
#endif

    public override void ExecuteRenderGraph(FullscreenPassDataBase passData, RasterGraphContext rgContext)
    {
        UpdateVolumeSettings();

        base.ExecuteRenderGraph(passData, rgContext);
    }
}
