Shader "Hidden/RippleDraw"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _RipplePos ("Ripple Position", Vector) = (0.5,0.5,0,0)
        _RippleSize ("Ripple Size", Float) = 0.05
        _RippleStrength ("Ripple Strength", Float) = 1
        _Decay ("Decay", Float) = 0.98
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZWrite Off
        Cull Off
        Blend One One

        Pass
        {
            HLSLPROGRAM
    #pragma vertex vert
    #pragma fragment frag

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);

    float4 _RipplePos;
    float _RippleSize;
    float _RippleStrength;
    float _Decay;

    v2f vert(appdata v)
    {
        v2f o;

        VertexPositionInputs posInputs = GetVertexPositionInputs(v.vertex.xyz);
        o.vertex = posInputs.positionCS;

        o.uv = v.uv;
        return o;
    }

    float4 frag(v2f i) : SV_Target
    {
        float current = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).r;

        float dist = distance(i.uv, _RipplePos.xy);

        float ripple = smoothstep(_RippleSize, 0, dist);

        float result = current * _Decay + ripple * _RippleStrength;

        return float4(result, result, result, 1);
    }

    ENDHLSL
        }
    }
}