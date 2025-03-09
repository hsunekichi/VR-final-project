// AlbedoTextureShader.hlsl
Shader "Custom/AlbedoTextureShader"
{
    Properties
    {
        _MainTex ("Albedo Texture", 2D) = "white" {}
        _ambientMultiplier("Ambient Multiplier", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Texture and sampler for albedo
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // Ambient multiplier
            float _ambientMultiplier;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                return albedo * _ambientMultiplier;
            }

            ENDHLSL
        }
    }
}
