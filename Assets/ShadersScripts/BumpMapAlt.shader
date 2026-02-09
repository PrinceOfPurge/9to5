Shader "BumpMapping"
{
    Properties
    {
        _myDiffuse ("Diffuse Texture", 2D) = "white" {}
        _myBump ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Range(0,10)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert
        #pragma target 3.0
        #include "UnityCG.cginc"

        sampler2D _myDiffuse;
        sampler2D _myBump;
        half _BumpScale;

        struct Input
        {
            float2 uv_myDiffuse;
            float2 uv_myBump;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo
            o.Albedo = tex2D(_myDiffuse, IN.uv_myDiffuse).rgb;

            // Sample and unpack the normal map (tangent-space normal)
            float3 n = UnpackNormal(tex2D(_myBump, IN.uv_myBump)).rgb;

            // Apply strength only to X/Y, then renormalize
            n.xy *= _BumpScale;
            n = normalize(n);

            // Assign the perturbed normal
            o.Normal = n;
        }
        ENDCG
    }
    Fallback "Diffuse"
}