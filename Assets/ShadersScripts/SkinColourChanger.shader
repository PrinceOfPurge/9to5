Shader "SkinMaskRecolor"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _MaskMap ("Mask Map", 2D) = "white" {}
        _SkinColor ("Skin Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

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
            // Base texture
            float4 baseCol = tex2D(_BaseMap, IN.uv_BaseMap);

            // Mask (white = skin, black = everything else)
            float mask = tex2D(_MaskMap, IN.uv_BaseMap).r;

            // Tint the skin while preserving hand-painted detail
            float3 tintedSkin = baseCol.rgb * _SkinColor.rgb;

            // Blend only where mask = 1
            float3 finalRGB = lerp(baseCol.rgb, tintedSkin, mask);

            o.Albedo = finalRGB;
            o.Alpha = 1;
        }
        ENDCG
    }
}