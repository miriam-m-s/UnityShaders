Shader "Unlit/S_lightModel"
{
    Properties
    {
       _Ambient ("Ambient Color", Range(0, 1)) = 1
       _LightInt ("Light Intensity", Range(0, 1)) = 1
       _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
           

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal_world : TEXCOORD1;
            };

            float _Ambient;
            float _LightInt;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.normal_world = normalize(mul(unity_ObjectToWorld,float4(v.normal, 0))).xyz;
                return o;

            }
            float3 LambertShading
                (
                    float3 colorRefl, // Dr
                    float lightInt, // Dl light intensity
                    float3 normal, // n
                    float3 lightDir // l  light direction
                )
            {
                return colorRefl * lightInt * max(0, dot(normal, lightDir));
            }
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float3 normal = i.normal_world;
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 ambient_color = UNITY_LIGHTMODEL_AMBIENT * _Ambient;
                col.rgb += ambient_color;
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                // LambertShading(1, 2, 3, 4);
                half3 diffuse = LambertShading( float3(1.0,1.0,1.0), _LightInt,normal, lightDir);
                col.rgb *= diffuse;

                return col;
            }
            ENDCG
        }
    }
}
