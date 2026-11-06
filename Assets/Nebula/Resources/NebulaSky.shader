Shader "Nebula Defender/Deep Space Sky"
{
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Input { float4 vertex : POSITION; };
            struct Output { float4 position : SV_POSITION; float3 direction : TEXCOORD0; };
            float4 _NebulaTint;
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            Output Vert(Input input)
            {
                Output output;
                output.position=TransformObjectToHClip(input.vertex.xyz);
                output.direction=mul((float3x3)unity_ObjectToWorld,input.vertex.xyz);
                return output;
            }
            half4 Frag(Output input) : SV_Target
            {
                float3 d=normalize(input.direction);
                float wave=sin(dot(d,float3(3.4,6.1,2.7))) * sin(dot(d,float3(7.2,-4.2,5.1)));
                float wisps=saturate(wave*.5+.45);
                wisps=wisps*wisps*wisps;
                float band=pow(saturate(1-abs(d.y+.12)*1.3),3);
                float3 color=float3(.001,.002,.008)+_NebulaTint.rgb*(wisps*.09+band*.015);
                // Cube-face projection avoids a dense pinwheel at the celestial poles.
                float3 axis=abs(d);
                float2 uv; float faceID;
                if(axis.y>axis.x && axis.y>axis.z) { uv=d.xz/axis.y; faceID=d.y>0?0:1; }
                else if(axis.x>axis.z) { uv=d.zy/axis.x; faceID=d.x>0?2:3; }
                else { uv=d.xy/axis.z; faceID=d.z>0?4:5; }
                float2 grid=uv*90;
                float2 cell=floor(grid)+float2(faceID*1031,faceID*947);
                float seed=Hash(cell);
                float2 starCenter=float2(Hash(cell+19.4),Hash(cell+83.7))*.7+.15;
                float radius=.025+Hash(cell+6.2)*.04;
                float distanceToStar=length(frac(grid)-starCenter);
                float pixelWidth=clamp(max(length(ddx(grid)),length(ddy(grid))),.015,.35);
                float star=(1-smoothstep(max(0,radius-pixelWidth*.4),radius+pixelWidth*.4,distanceToStar))*step(.993,seed);
                float starColor=Hash(cell+17.3);
                color+=star*lerp(float3(.45,.65,1),float3(1,.95,.85),starColor)*(.35+starColor*.45);
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
