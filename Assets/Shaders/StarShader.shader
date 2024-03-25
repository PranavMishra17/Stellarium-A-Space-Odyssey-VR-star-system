Shader "Unlit/StarShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Size ("Size", float) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha // Use standard alpha blending
        ZTest Off
        ZWrite Off // Disable Z writing to allow for transparency

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing // Enable GPU instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float, _Size)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex * UNITY_ACCESS_INSTANCED_PROP(Props, _Size));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float distance_from_centre = length((2 * i.uv) - 1);
                float inverse_dist = saturate((0.2 / distance_from_centre) - 0.2);
                fixed4 col = UNITY_ACCESS_INSTANCED_PROP(Props, _Color) * inverse_dist;
                // Ensure alpha falls off to 0 at the edges
                col.a = inverse_dist;
                return col;
            }
            ENDCG
        }
    }
}
