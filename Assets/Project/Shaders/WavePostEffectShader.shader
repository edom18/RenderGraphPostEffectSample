Shader "Hidden/WavePostEffectShader"
{
    Properties
    {
        _Wave ("Wave", Float) = 30.0
        _Speed ("Speed", Float) = 10.0
        _Offset ("Offset", Vector) = (0, 0, 0, 0)
        _Intensity ("Intensity", Float) = 0.0
        _NeedsEffect ("Needs Effect", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "WaveEffect"
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            #pragma vertex Vert
            #pragma fragment Frag

            float _Wave;
            float _Speed;
            float _Intensity;
            int _NeedsEffect;
            float2 _Offset;

            float getHeight(float2 uv)
            {
                float d = length(uv);
                return sin(d - _Time.x * _Speed);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord.xy;

                if (_NeedsEffect == 0)
                {
                    return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                }

                const float e = 0.01;
                float2 shiftX = float2(e, 0);
                float2 shiftY = float2(0, e);

                float aspectRatio = _ScreenParams.x / _ScreenParams.y;
                float2 effectUv = (uv * 2.0 - 1.0);
                effectUv -= _Offset.xy;
                effectUv.x *= aspectRatio;
                float distanceThreshold = clamp(1.0 - pow(length(effectUv), 3.0), 0.0, 1.0);
                
                effectUv *= _Wave;
                float hX = getHeight(effectUv + shiftX);
                float hx = getHeight(effectUv - shiftX);
                float hY = getHeight(effectUv + shiftY);
                float hy = getHeight(effectUv - shiftY);
                
                float yu = (hX - hx) * 0.5;
                float yv = (hY - hy) * 0.5;
                float3 du = float3(e, yu, 0);
                float3 dv = float3(0, yv, e);

                
                float3 n = normalize(cross(dv, du));
                uv += n.xz * 0.05 * _Intensity * distanceThreshold;
                
                half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                return col;
            }
            ENDHLSL
        }
    }
}
