Shader "Custom/RoundedCorner"
{
    Properties
    {
        _MainTex    ("Texture",    2D)    = "white" {}
        _Radius     ("Radius",     Float) = 20
        _Width      ("Width",      Float) = 370
        _Height     ("Height",     Float) = 208
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f    { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };

            sampler2D _MainTex;
            float _Radius;
            float _Width;
            float _Height;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = v.uv;
                o.color = v.color;
                return o;
            }

            float RoundedAlpha(float2 uv, float2 size, float radius)
            {
                float2 pixel  = uv * size;
                float2 corner = clamp(pixel, float2(radius, radius), size - float2(radius, radius));
                float  dist   = length(pixel - corner);
                return 1.0 - smoothstep(radius - 1.0, radius, dist);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                col.a *= RoundedAlpha(i.uv, float2(_Width, _Height), _Radius);
                return col;
            }
            ENDCG
        }
    }
}
