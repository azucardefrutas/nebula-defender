Shader "Nebula Defender/Shield Rim"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 p:POSITION; float3 n:NORMAL; };
            struct Varyings { float4 p:SV_POSITION; float3 n:TEXCOORD0; float3 w:TEXCOORD1; };
            Varyings vert(Attributes v) { Varyings o; o.p=TransformObjectToHClip(v.p.xyz); o.n=TransformObjectToWorldNormal(v.n); o.w=TransformObjectToWorld(v.p.xyz); return o; }
            half4 frag(Varyings i):SV_Target
            {
                float3 n=normalize(i.n), v=normalize(GetCameraPositionWS()-i.w);
                float rim=pow(1-saturate(dot(n,v)),3);
                float scan=pow(saturate(sin(i.w.y*19-_Time.y*4)),20)*.07;
                return half4(.08,.65,1.3,saturate(rim*.65+scan+0.006));
            }
            ENDHLSL
        }
    }
}
