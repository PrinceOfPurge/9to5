Shader "SkinMaskRecolor"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _MaskMap ("Mask Map", 2D) = "white" {}
        _SkinColor ("Skin Color", Color) = (1,1,1,1)

        _OutlineColor ("Outline Colour", Color) = (0,0,0,1)
        _Outline ("Outline Width", Range (.00001, 0.1)) = .02
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _BaseMap;
        sampler2D _MaskMap;
        float4 _SkinColor;

        struct Input
        {
            float2 uv_BaseMap;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 baseCol = tex2D(_BaseMap, IN.uv_BaseMap);
            float mask = tex2D(_MaskMap, IN.uv_BaseMap).r;

            float3 tinted = baseCol.rgb * _SkinColor.rgb;
            float3 finalRGB = lerp(baseCol.rgb, tinted, mask);

            o.Albedo = finalRGB;
            o.Alpha = 1;
        }
        ENDCG

        Pass
        {
            Name "Outline"
            Cull Front
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Outline;
            float4 _OutlineColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float3 n = normalize(v.normal);
                v.vertex.xyz += n * _Outline;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
            ZWrite On
        }
    }

    FallBack "Diffuse"
}