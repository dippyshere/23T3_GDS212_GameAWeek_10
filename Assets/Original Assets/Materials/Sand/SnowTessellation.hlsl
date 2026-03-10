//#ifndef TESSELLATION_CGINC_INCLUDED
//#define TESSELLATION_CGINC_INCLUDED
#if defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_VULKAN) || defined(SHADER_API_METAL) || defined(SHADER_API_PSSL)
#define UNITY_CAN_COMPILE_TESSELLATION 1
#   define UNITY_domain                 domain
#   define UNITY_partitioning           partitioning
#   define UNITY_outputtopology         outputtopology
#   define UNITY_patchconstantfunc      patchconstantfunc
#   define UNITY_outputcontrolpoints    outputcontrolpoints
#endif

// Shadow Casting Light geometric parameters. These variables are used when applying the shadow Normal Bias and are set by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
// For Directional lights, _LightDirection is used when applying shadow Normal Bias.
// For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different at each shadow caster geometry vertex.
float3 _LightDirection;
float3 _LightPosition;

struct Varyings2
{
    float3 worldPos : TEXCOORD1;
    float3 normal : NORMAL;
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 viewDir : TEXCOORD3;
    float fogFactor : TEXCOORD4;
    float2 staticLightmapUV : TEXCOORD6;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

float _Tess;
float _MaxTessDistance;

struct TessellationFactors
{
    float edge[3] : SV_TessFactor;
    float inside : SV_InsideTessFactor;
};

struct Attributes2
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 uv : TEXCOORD0;
    float2 staticLightmapUV : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct ControlPoint
{
    float4 vertex : INTERNALTESSPOS;
    float2 uv : TEXCOORD0;
    float2 staticLightmapUV : TEXCOORD1;
    float3 normal : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
[UNITY_partitioning("fractional_odd")]
[UNITY_patchconstantfunc("patchConstantFunction")]
ControlPoint hull(InputPatch<ControlPoint, 3> patch, uint id : SV_OutputControlPointID)
{
    return patch[id];
}

TessellationFactors UnityCalcTriEdgeTessFactors(float3 triVertexFactors)
{
    TessellationFactors tess;
    tess.edge[0] = 0.5 * (triVertexFactors.y + triVertexFactors.z);
    tess.edge[1] = 0.5 * (triVertexFactors.x + triVertexFactors.z);
    tess.edge[2] = 0.5 * (triVertexFactors.x + triVertexFactors.y);
    tess.inside = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
    return tess;
}

float CalcDistanceTessFactor(float4 vertex, float minDist, float maxDist, float tess)
{
    float3 worldPosition = mul(UNITY_MATRIX_M, vertex).xyz;
    float dist = distance(worldPosition, _WorldSpaceCameraPos);
    float f = clamp(1.0 - (dist - minDist) / maxDist, 0, 1.0);
    return f * tess + 1;
}

TessellationFactors DistanceBasedTess(float4 v0, float4 v1, float4 v2, float minDist, float maxDist, float tess)
{
    float3 f;
    f.x = CalcDistanceTessFactor(v0, minDist, maxDist, tess);
    f.y = CalcDistanceTessFactor(v1, minDist, maxDist, tess);
    f.z = CalcDistanceTessFactor(v2, minDist, maxDist, tess);

    return UnityCalcTriEdgeTessFactors(f);
}

uniform float3 _Position;
uniform sampler2D _GlobalEffectRT;
uniform float _OrthographicCamSize;

sampler2D _Noise;
float _NoiseScale, _SnowHeight, _NoiseWeight, _SnowDepth;

#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
#define ENABLE_TERRAIN_PERPIXEL_NORMAL
#endif

#ifdef UNITY_INSTANCING_ENABLED
CBUFFER_START(_Terrain)
float4 _TerrainHeightmapRecipSize;
float4 _TerrainHeightmapScale;
CBUFFER_END

TEXTURE2D(_TerrainHeightmapTexture);
TEXTURE2D(_TerrainNormalmapTexture);

UNITY_INSTANCING_BUFFER_START(Terrain)
UNITY_DEFINE_INSTANCED_PROP(float4, _TerrainPatchInstanceData)
UNITY_INSTANCING_BUFFER_END(Terrain)
#endif

void TerrainInstancing(inout float4 positionOS, inout float3 normalOS, inout float2 uv)
{
#ifdef UNITY_INSTANCING_ENABLED
    float2 patchVertex = positionOS.xy;
    float4 instanceData = UNITY_ACCESS_INSTANCED_PROP(Terrain, _TerrainPatchInstanceData);
    float2 sampleCoords = (patchVertex + instanceData.xy) * instanceData.z;

    float height = UnpackHeightmap(_TerrainHeightmapTexture.Load(int3(sampleCoords, 0)));

    positionOS.xz = sampleCoords * _TerrainHeightmapScale.xz;
    positionOS.y = height * _TerrainHeightmapScale.y;

    #ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
    normalOS = float3(0, 1, 0);
    #else
    normalOS = _TerrainNormalmapTexture.Load(int3(sampleCoords, 0)).rgb * 2 - 1;
    #endif

    uv = sampleCoords * _TerrainHeightmapRecipSize.zw;
#endif
}

TessellationFactors patchConstantFunction(InputPatch<ControlPoint, 3> patch)
{
    UNITY_SETUP_INSTANCE_ID(patch[0]);
    float minDist = 2.0;
    float maxDist = _MaxTessDistance;
    TessellationFactors f;
    return DistanceBasedTess(patch[0].vertex, patch[1].vertex, patch[2].vertex, minDist, maxDist, _Tess);
}

float4 GetShadowPositionHClip2(Attributes2 input)
{
    float3 positionWS = TransformObjectToWorld(input.vertex.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normal);

    #if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
    #else
    float3 lightDirectionWS = _LightDirection;
    #endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
    positionCS = ApplyShadowClamping(positionCS);
    return positionCS;
}


Varyings2 vert(Attributes2 input)
{
    Varyings2 output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    float3 worldPosition = TransformObjectToWorld(input.vertex.xyz);
    //create local uv
    float2 uv = worldPosition.xz - _Position.xz;
    uv = uv / (_OrthographicCamSize * 2);
    uv += 0.5;

    // Effects RenderTexture Reading
    float4 RTEffect = tex2Dlod(_GlobalEffectRT, float4(uv, 0, 0));
    // smoothstep to prevent bleeding
    RTEffect *= smoothstep(0.99, 0.9, uv.x) * smoothstep(0.99, 0.9, 1 - uv.x);
    RTEffect *= smoothstep(0.99, 0.9, uv.y) * smoothstep(0.99, 0.9, 1 - uv.y);

    // worldspace noise texture
    float SnowNoise = tex2Dlod(_Noise, float4(worldPosition.xz * _NoiseScale, 0, 0)).r;

    // move vertices up where snow is
    input.vertex.xyz += SafeNormalize(input.normal) * saturate(_SnowHeight + SnowNoise * _NoiseWeight) * saturate(
        1 - RTEffect.g * _SnowDepth);
    // input.vertex.xyz +=  step(0.5,RTEffect.g) * input.normal;

    // transform to clip space
    #ifdef SHADERPASS_SHADOWCASTER
    output.vertex = GetShadowPositionHClip2(input);
    #else
    output.vertex = TransformObjectToHClip(input.vertex.xyz);
    #endif

    //outputs
    output.worldPos = TransformObjectToWorld(input.vertex.xyz);
    output.viewDir = SafeNormalize(GetCameraPositionWS() - output.worldPos);
    output.normal = TransformObjectToWorldNormal(input.normal);
    output.uv = input.uv;
    // Terrain instancing generates terrain-space UV in input.uv, which is the correct source for LM sampling.
    #if defined(UNITY_INSTANCING_ENABLED)
    output.staticLightmapUV = input.uv * unity_LightmapST.xy + unity_LightmapST.zw;
    #else
    output.staticLightmapUV = input.staticLightmapUV * unity_LightmapST.xy + unity_LightmapST.zw;
    #endif
    output.fogFactor = ComputeFogFactor(output.vertex.z);
    return output;
}

[UNITY_domain("tri")]
Varyings2 domain(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch,
                 float3 barycentricCoordinates : SV_DomainLocation)
{
    Attributes2 v;

    #define Interpolate(fieldName) v.fieldName = \
				patch[0].fieldName * barycentricCoordinates.x + \
				patch[1].fieldName * barycentricCoordinates.y + \
				patch[2].fieldName * barycentricCoordinates.z;

    Interpolate(vertex)
    Interpolate(uv)
    Interpolate(staticLightmapUV)
    Interpolate(normal)

    UNITY_TRANSFER_INSTANCE_ID(patch[0], v);
    return vert(v);
}
