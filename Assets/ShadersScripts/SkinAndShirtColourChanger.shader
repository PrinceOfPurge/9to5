Shader "SkinShirtRecolor"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _SkinMask ("Skin Mask", 2D) = "white" {}
        _ShirtMask ("Shirt Mask", 2D) = "white" {}

        _SkinColor ("Skin Color", Color) = (1,1,1,1)
        _ShirtColor ("Shirt Color", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Outline ("Outline Width", Range(0.00001, 0.1)) = 0.02
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _BaseMap;
        sampler2D _SkinMask;
        sampler2D _ShirtMask;

        float4 _SkinColor;
        float4 _ShirtColor;

        struct Input
        {
            float2 uv_BaseMap;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 baseCol = tex2D(_BaseMap, IN.uv_BaseMap);

            float skinMask  = tex2D(_SkinMask, IN.uv_BaseMap).r;
            float shirtMask = tex2D(_ShirtMask, IN.uv_BaseMap).r;

            float3 tintedSkin  = baseCol.rgb * _SkinColor.rgb;
            float3 tintedShirt = baseCol.rgb * _ShirtColor.rgb;

            float3 afterSkin  = lerp(baseCol.rgb, tintedSkin,  skinMask);
            float3 afterShirt = lerp(afterSkin,  tintedShirt, shirtMask);

            o.Albedo = afterShirt;
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