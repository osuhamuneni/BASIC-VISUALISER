Shader "Unlit/GalaxyCutShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale("Scale", Float) = 1
        _Offset("Offset", Vector) = (0,0,0,0)
        _Channel ("Channel", Range(0, 3)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float2 _Offset;
            float _Scale;
            int _Channel; // 0: All, 1: rR, 2: G, 3: B

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv -= 0.5;
                uv *= _Scale / 1000;
                uv += 0.5;
                fixed4 col = tex2D(_MainTex, (uv + _Offset) - _MainTex_TexelSize.xy*150);

                if (_Channel == 1) // Display only R channel
                    col = fixed4(col.r, col.r, col.r, col.a);
                else if (_Channel == 2) // Display only G channel
                    col = fixed4(col.g, col.g, col.g, col.a);
                else if (_Channel == 3) // Display only B channel
                    col = fixed4(col.b, col.b, col.b, col.a);

                return col;
            }
            ENDCG
        }
    }
}
