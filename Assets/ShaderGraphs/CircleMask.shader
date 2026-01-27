Shader "UI/CircleMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Range(0, 1)) = 0
        _Color ("Color", Color) = (0, 0, 0, 1)
    }
    
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Center;
            float _Radius;
            float _Soft;
            fixed4 _Color;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_base v){
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;
                
                float2 uv = i.uv;
                float2 center = _Center.xy;
                
                uv.x = (uv.x - 0.5) * aspect + 0.5;
                center.x = (center.x - 0.5) * aspect + 0.5;

                float2 corners[4] = {
                    float2((0 - 0.5) * aspect + 0.5, 0),
                    float2((1 - 0.5) * aspect + 0.5, 0),
                    float2((0 - 0.5) * aspect + 0.5, 1),
                    float2((1 - 0.5) * aspect + 0.5, 1)
                };

                float maxD = 0;
                for (int k = 0; k < 4; k++)
                {
                    maxD = max(maxD, distance(corners[k], center));
                }
                
                float d = distance(uv, center);
                float mask = step(d, _Radius * maxD);
                
                return fixed4(_Color.rgb, 1 - mask);
            }
            ENDCG
        }
    }
}
