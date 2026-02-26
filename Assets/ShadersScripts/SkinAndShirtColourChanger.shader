Shader "SkinShirtRecolor"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _SkinMask ("Skin Mask", 2D) = "white" {}
        _ShirtMask ("Shirt Mask", 2D) = "white" {}

        _SkinColor ("Skin Color", Color) = (1,1,1,1)
        _ShirtColor ("Shirt Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard

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

            // Tint while preserving detail
            float3 tintedSkin  = baseCol.rgb * _SkinColor.rgb;
            float3 tintedShirt = baseCol.rgb * _ShirtColor.rgb;

            // Apply masks
            float3 afterSkin  = lerp(baseCol.rgb, tintedSkin,  skinMask);
            float3 afterShirt = lerp(afterSkin,  tintedShirt, shirtMask);

            o.Albedo = afterShirt;
            o.Alpha = 1;
        }
        ENDCG
    }
}