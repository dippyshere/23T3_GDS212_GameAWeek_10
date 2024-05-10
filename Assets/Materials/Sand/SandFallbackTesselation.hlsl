struct Varyings
{
    float3 worldPos : TEXCOORD1;
    float3 normal : NORMAL;
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 viewDir : TEXCOORD3;
    float fogFactor : TEXCOORD4;
};

uniform float3 _Position;
uniform sampler2D _GlobalEffectRT;
uniform float _OrthographicCamSize;

sampler2D  _Noise;
float _NoiseScale, _SnowHeight, _NoiseWeight, _SnowDepth;

float4 GetShadowPositionHClip(float4 vertex, float3 normal)
{
    float3 positionWS = mul(unity_ObjectToWorld, vertex.xyz).xyz;
    float3 normalWS = mul(unity_ObjectToWorld, float4(normal, 0)).xyz;

    float4 positionCS = mul(UNITY_MATRIX_VP, float4(positionWS, 1));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif
    return positionCS;
}


Varyings vert(float4 vertex : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0)
{
    Varyings output;

    float3 worldPosition = mul(unity_ObjectToWorld, vertex).xyz;
    // Create local uv
    float2 localUV = worldPosition.xz - _Position.xz;
    localUV /= (_OrthographicCamSize * 2);
    localUV += 0.5;

    // Effects RenderTexture Reading
    float4 RTEffect = tex2Dlod(_GlobalEffectRT, float4(localUV, 0, 0));
    // Smoothstep to prevent bleeding
    RTEffect *= smoothstep(0.99, 0.9, localUV.x) * smoothstep(0.99, 0.9, 1 - localUV.x);
    RTEffect *= smoothstep(0.99, 0.9, localUV.y) * smoothstep(0.99, 0.9, 1 - localUV.y);

    // Worldspace noise texture
    float SnowNoise = tex2Dlod(_Noise, float4(worldPosition.xz * _NoiseScale, 0, 0)).r;
    output.viewDir = normalize(_WorldSpaceCameraPos - worldPosition);

    // Move vertices up where snow is
    //vertex.xyz += normalize(normal) * saturate((_SnowHeight)+(SnowNoise * _NoiseWeight));

    // Transform to clip space
    //output.vertex = GetShadowPositionHClip(vertex, normal);

    // Outputs
    output.worldPos = worldPosition;
    output.normal = normal;
    output.uv = uv;
	output.vertex = mul(UNITY_MATRIX_MVP, vertex);
    //output.fogFactor = ComputeFogFactor(output.vertex.z);
    return output;
}

Varyings vert(float4 vertex : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0);

