Shader "Nebula Defender/Nebula"
{
    Properties { _BaseColor("Base",Color)=(0.01,0.02,0.05,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Cull Off ZWrite On
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            half4 _NebulaTint;
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float noise(float2 p)
            {
                float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);
            }
            Varyings vert(Attributes v) { Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;return o; }
            half4 frag(Varyings i):SV_Target
            {
                float2 p=(i.uv-0.5)*8+float2(_Time.y*0.0015,0);
                float cloud=noise(p)*0.55+noise(p*2.07+3)*0.28+noise(p*4.1)*0.12;
                float distanceToBand=(p.y+p.x*0.42+noise(p*0.65)*1.8)*0.85;
                float band=exp(-distanceToBand*distanceToBand);
                half3 tint=max(_NebulaTint.rgb,half3(0.04,0.12,0.24));
                half3 color=_BaseColor.rgb+tint*pow(saturate(cloud),2.5)*band*0.52;
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
