Shader "Custom/Snow Interactive NoTess"
{
    Properties
    {
        [Header(Main)]
        _Noise("Snow Noise", 2D) = "gray" {}
        _NoiseScale("Noise Scale", Range(0,2)) = 0.1
        _NoiseWeight("Noise Weight", Range(0,2)) = 0.1
        [HDR]_ShadowColor("Shadow Color", Color) = (0.5,0.5,0.5,1)

        [Space]
        [Header(Tesselation)]
        _MaxTessDistance("Max Tessellation Distance", Range(10,100)) = 50
        _Tess("Tessellation", Range(1,512)) = 20

        [Space]
        [Header(Snow)]
        [HDR]_Color("Snow Color", Color) = (0.5,0.5,0.5,1)
        [HDR]_PathColorIn("Snow Path Color In", Color) = (0.5,0.5,0.7,1)
        [HDR]_PathColorOut("Snow Path Color Out", Color) = (0.5,0.5,0.7,1)
        _PathBlending("Snow Path Blending", Range(0,3)) = 0.3
        _MainTex("Snow Texture", 2D) = "white" {}
        _SnowHeight("Snow Height", Range(0,2)) = 0.3
        _SnowDepth("Snow Path Depth", Range(-2,2)) = 0.3
        _SnowTextureOpacity("Snow Texture Opacity", Range(0,1)) = 0.3
        _SnowTextureScale("Snow Texture Scale", Range(0,2)) = 0.3

        [Space]
        [Header(Sparkles)]
        _SparkleScale("Sparkle Scale", Range(0,10)) = 10
        _SparkCutoff("Sparkle Cutoff", Range(0,2)) = 0.8
        _SparkleNoise("Sparkle Noise", 2D) = "gray" {}

        [Space]
        [Header(Rim)]
        _RimPower("Rim Power", Range(0,20)) = 20
        [HDR]_RimColor("Rim Color Snow", Color) = (0.5,0.5,0.5,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

        // Shadow Casting Light geometric parameters
        float3 _LightDirection;
        float3 _LightPosition;

        uniform float3 _Position;
        uniform sampler2D _GlobalEffectRT;
        uniform float _OrthographicCamSize;

        sampler2D _Noise;
        float _NoiseScale, _SnowHeight, _NoiseWeight, _SnowDepth;
        float _Tess, _MaxTessDistance;

        struct Attributes
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float2 uv : TEXCOORD0;
            float2 staticLightmapUV : TEXCOORD1;
            float4 tangent : TANGENT;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 worldPos : TEXCOORD0;
            float2 uv : TEXCOORD1;
            float3 normal : TEXCOORD2;
            float3 viewDir : TEXCOORD3;
            float fogFactor : TEXCOORD4;
            float3 tangent : TEXCOORD5;
            float3 bitangent : TEXCOORD6;
            float2 staticLightmapUV : TEXCOORD7;
        };

        Varyings SnowVert(Attributes input)
        {
            Varyings output = (Varyings)0;

            float3 worldPosition = mul(unity_ObjectToWorld, input.vertex).xyz;

            // create local uv for effect RT
            float2 uv = worldPosition.xz - _Position.xz;
            uv = uv / (_OrthographicCamSize * 2);
            uv += 0.5;

            // Effects RenderTexture Reading
            float4 RTEffect = tex2Dlod(_GlobalEffectRT, float4(uv, 0, 0));
            RTEffect *= smoothstep(0.99, 0.9, uv.x) * smoothstep(0.99, 0.9, 1 - uv.x);
            RTEffect *= smoothstep(0.99, 0.9, uv.y) * smoothstep(0.99, 0.9, 1 - uv.y);

            // worldspace noise texture
            float SnowNoise = tex2Dlod(_Noise, float4(worldPosition.xz * _NoiseScale, 0, 0)).r;

            // move vertices up where snow is
            input.vertex.xyz += SafeNormalize(input.normal) * saturate(_SnowHeight + SnowNoise * _NoiseWeight) *
                saturate(1 - RTEffect.g * _SnowDepth);

            // transform to clip space
            #ifdef SHADERPASS_SHADOWCASTER
            float3 positionWS = TransformObjectToWorld(input.vertex.xyz);
            float3 normalWS = input.normal;
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
            float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
            float3 lightDirectionWS = _LightDirection;
            #endif
            output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, 1));
            output.positionCS = ApplyShadowClamping(output.positionCS);
            #else
            output.positionCS = TransformObjectToHClip(input.vertex.xyz);
            #endif

            output.worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
            output.viewDir = SafeNormalize(GetCameraPositionWS() - output.worldPos);
            output.normal = input.normal;
            output.tangent = input.tangent.xyz;
            output.bitangent = cross(input.tangent.xyz, output.normal);
            output.uv = input.uv;
            output.staticLightmapUV = input.staticLightmapUV * unity_LightmapST.xy + unity_LightmapST.zw;
            output.fogFactor = ComputeFogFactor(output.positionCS.z);
            return output;
        }
        ENDHLSL

        // Forward Pass
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex SnowVert
            #pragma fragment frag
            #pragma target 3.0
            #pragma instancing_options renderinglayer

            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            sampler2D _MainTex, _SparkleNoise;
            float4 _Color, _RimColor;
            float _RimPower;
            float4 _PathColorIn, _PathColorOut;
            float _PathBlending;
            float _SparkleScale, _SparkCutoff;
            float _SnowTextureOpacity, _SnowTextureScale;
            float4 _ShadowColor;

            half4 frag(Varyings IN) : SV_Target
            {
                // Effects RenderTexture Reading
                float2 uv = IN.worldPos.xz - _Position.xz;
                uv /= _OrthographicCamSize * 2;
                uv += 0.5;

                // effects texture
                float4 effect = tex2D(_GlobalEffectRT, uv);

                // mask to prevent bleeding
                effect *= smoothstep(0.99, 0.9, uv.x) * smoothstep(0.99, 0.9, 1 - uv.x);
                effect *= smoothstep(0.99, 0.9, uv.y) * smoothstep(0.99, 0.9, 1 - uv.y);

                // worldspace Noise texture
                float3 topdownNoise = tex2D(_Noise, IN.worldPos.xz * _NoiseScale).rgb;

                // worldspace Snow texture
                float3 snowtexture = tex2D(_MainTex, IN.worldPos.xz * _SnowTextureScale).rgb;

                // lerp between snow color and snow texture
                float3 snowTex = lerp(_Color.rgb, snowtexture * _Color.rgb, _SnowTextureOpacity);

                // lerp the colors using the RT effect path
                float3 path = lerp(_PathColorOut.rgb * effect.g, _PathColorIn.rgb, saturate(effect.g * _PathBlending));
                float3 mainColors = lerp(snowTex, path, saturate(effect.g));

                // Baked GI / ambient lighting
                half3 bakedGI = half3(0, 0, 0);
                #if defined(LIGHTMAP_ON)
                bakedGI = SampleLightmap(IN.staticLightmapUV, IN.normal);
                #else
                bakedGI = unity_AmbientSky.rgb;
                #endif

                // Shadow mask for baked shadow support
                half4 shadowMask = half4(1, 1, 1, 1);
                #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
                shadowMask = SAMPLE_SHADOWMASK(IN.staticLightmapUV);
                #endif

                // lighting and shadow information
                half shadow = 0;
                half4 shadowCoord = TransformWorldToShadowCoord(IN.worldPos);

                #if _MAIN_LIGHT_SHADOWS_CASCADE || _MAIN_LIGHT_SHADOWS || _MAIN_LIGHT_SHADOWS_SCREEN
                Light mainLight = GetMainLight(shadowCoord, IN.worldPos, shadowMask);
                shadow = mainLight.shadowAttenuation;
                #else
                Light mainLight = GetMainLight();
                #endif

                MixRealtimeAndBakedGI(mainLight, IN.normal, bakedGI);

                // add in the sparkles
                float sparklesStatic = tex2D(_SparkleNoise, IN.worldPos.xz * _SparkleScale).r;
                half cutoffSparkles = step(_SparkCutoff, sparklesStatic);
                mainColors += lerp(cutoffSparkles * 4, 0, saturate(effect.g * 2));

                // add rim light
                half rim = 1.0 - dot(IN.viewDir, IN.normal) * topdownNoise.r;
                // no rim inside of the path
                rim = lerp(rim, 0, saturate(effect.g));
                mainColors += _RimColor * pow(abs(rim), _RimPower);

                return half4(MixFog(mainColors * mainLight.color.rgb * (shadow + bakedGI), IN.fogFactor), 1);
            }
            ENDHLSL
        }

        // Shadow Casting Pass
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #define SHADERPASS_SHADOWCASTER
            #pragma vertex SnowVert
            #pragma fragment fragShadow
            #pragma target 3.0
            #pragma multi_compile_shadowcaster

            half4 fragShadow(Varyings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // Depth Only Pass
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex SnowVert
            #pragma fragment fragDepth
            #pragma target 3.0

            half4 fragDepth(Varyings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // Depth Normals Pass
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex SnowVert
            #pragma fragment fragDepthNormals
            #pragma target 3.0

            half4 fragDepthNormals(Varyings IN) : SV_Target
            {
                float3 normalWS = normalize(IN.normal);
                return half4(normalWS, 0);
            }
            ENDHLSL
        }

        // Meta pass for lightmap baking
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }
            Cull Off

            HLSLPROGRAM
            #pragma vertex MetaVert
            #pragma fragment MetaFrag
            #pragma target 3.0
            #pragma multi_compile_fragment _ EDITOR_VISUALIZATION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            sampler2D _MainTex;
            float4 _Color;
            float _SnowTextureOpacity, _SnowTextureScale;

            struct MetaVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                #ifdef EDITOR_VISUALIZATION
                float2 vizUV : TEXCOORD2;
                float4 lightCoord : TEXCOORD3;
                #endif
            };

            MetaVaryings MetaVert(Attributes input)
            {
                MetaVaryings output = (MetaVaryings)0;
                output.positionCS = MetaVertexPosition(input.vertex, input.staticLightmapUV, float2(0, 0),
                                                       unity_LightmapST, unity_DynamicLightmapST);
                output.uv = input.uv;
                output.worldPos = TransformObjectToWorld(input.vertex.xyz);
                #ifdef EDITOR_VISUALIZATION
                UnityEditorVizData(input.vertex.xyz, input.uv, input.staticLightmapUV, float2(0, 0), output.vizUV,
                    output.lightCoord);
                #endif
                return output;
            }

            half4 MetaFrag(MetaVaryings IN) : SV_Target
            {
                MetaInput metaInput;
                float3 snowtexture = tex2D(_MainTex, IN.worldPos.xz * _SnowTextureScale).rgb;
                float3 snowTex = lerp(_Color.rgb, snowtexture * _Color.rgb, _SnowTextureOpacity);

                metaInput.Albedo = snowTex;
                metaInput.Emission = 0;
                #ifdef EDITOR_VISUALIZATION
                metaInput.VizUV = IN.vizUV;
                metaInput.LightCoord = IN.lightCoord;
                #endif
                return MetaFragment(metaInput);
            }
            ENDHLSL
        }
    }
}