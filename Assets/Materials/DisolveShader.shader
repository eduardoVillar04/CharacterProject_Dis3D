Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DissolveAmount("Dissolve Amount", Range(0,1)) = 0.0
        _DissolveMap("texture",2D) = "white" {}
        _EdgeWidth("Edge width", Float) = 0.1
        _EdgeColor("Edge color", Color) = (1,0,0,1)
        _EdgeGlowStrength("Edge Glow Strength", Float) = 1.0
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float4 _EdgeColor;
            float _DissolveAmount;
            sampler2D _DissolveMap;
            float _EdgeWidth;
            float _EdgeGlowStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float cutout = tex2D(_DissolveMap, i.uv);
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                if(cutout < _DissolveAmount)
                {
                    discard;
                }

                if(cutout < col.a && cutout < _DissolveAmount + _EdgeWidth)
                {
                    col = lerp(_EdgeColor * _EdgeGlowStrength, col, (cutout - _DissolveAmount) / _EdgeWidth);
                }

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
