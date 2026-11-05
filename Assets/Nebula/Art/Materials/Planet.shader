Shader "Nebula Defender/Planet"
{
    Properties { _BaseColor("Ocean",Color)=(0.015,0.16,0.32,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes { float4 p:POSITION;float3 n:NORMAL; };
            struct Varyings { float4 p:SV_POSITION;float3 n:TEXCOORD0;float3 w:TEXCOORD1;float3 o:TEXCOORD2; };
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            Varyings vert(Attributes v) { Varyings o;o.p=TransformObjectToHClip(v.p.xyz);o.n=TransformObjectToWorldNormal(v.n);o.w=TransformObjectToWorld(v.p.xyz);o.o=v.p.xyz;return o; }
            half4 frag(Varyings i):SV_Target
            {
                float3 n=normalize(i.n),v=normalize(GetCameraPositionWS()-i.w);
                float3 p=normalize(i.o);
                float continent=sin(p.x*17+sin(p.z*12))*sin(p.y*14+p.x*6)+sin(p.z*28+p.y*5)*0.25;
                float land=smoothstep(0.15,0.3,continent);
                half3 surface=lerp(_BaseColor.rgb,half3(0.035,0.25,0.23),land);
                float clouds=smoothstep(0.74,0.98,sin(p.y*37+sin(p.x*17))*sin(p.z*30));
                surface=lerp(surface,half3(0.55,0.75,0.85),clouds*0.65);
                Light light=GetMainLight();float diffuse=saturate(dot(n,light.direction));
                half3 color=surface*(0.13+diffuse*1.5)*light.color;
                color+=half3(0.05,0.3,0.65)*pow(1-saturate(dot(n,v)),3)*0.8;
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
