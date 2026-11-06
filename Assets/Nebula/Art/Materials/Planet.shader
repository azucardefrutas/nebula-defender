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
            float Hash(float3 p) { p=frac(p*.1031);p+=dot(p,p.yzx+33.33);return frac((p.x+p.y)*p.z); }
            float Noise(float3 p)
            {
                float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(Hash(a),Hash(a+float3(1,0,0)),f.x),lerp(Hash(a+float3(0,1,0)),Hash(a+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(Hash(a+float3(0,0,1)),Hash(a+float3(1,0,1)),f.x),lerp(Hash(a+float3(0,1,1)),Hash(a+1),f.x),f.y),f.z);
            }
            half4 frag(Varyings i):SV_Target
            {
                float3 n=normalize(i.n),v=normalize(GetCameraPositionWS()-i.w);
                float3 p=normalize(i.o);
                float continent=Noise(p*3.2)*.6+Noise(p*7.7+4)*.27+Noise(p*18)*.13;
                float land=smoothstep(.48,.52,continent);
                half3 surface=lerp(half3(.008,.055,.16),half3(.035,.16,.12),land);
                float clouds=smoothstep(.49,.69,Noise(p*8+float3(_Time.y*.006,0,0))*.7+Noise(p*23)*.3);
                surface=lerp(surface,half3(.65,.75,.87),clouds*.7);
                Light light=GetMainLight();float diffuse=saturate(dot(n,light.direction));
                half3 color=surface*(.035+diffuse*1.35)*light.color;
                color+=half3(.08,.32,.75)*pow(1-saturate(dot(n,v)),4)*(.18+diffuse*.7);
                color+=pow(saturate(dot(reflect(-light.direction,n),v)),70)*.15*(1-land);
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
