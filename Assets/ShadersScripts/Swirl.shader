Shader "WaterSwirl"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Tint("Colour Tint", Color) = (1,1,1,1)
        _Freq("Frequency", Range(0,5)) = 3
        _Speed("Speed", Range(0,100)) = 10
        _Amp("Amplitude", Range(0,1)) = 0.5
    }

    SubShader
    {
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert

        struct Input
        {
            float2 uv_MainTex;
            float3 vertColor;
        };

        float4 _Tint;
        float _Freq;
        float _Speed;
        float _Amp;

        struct appdata
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
        };

        void vert (inout appdata v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            float t = _Time.y * _Speed;

            // Position in XZ plane
            float2 pos = v.vertex.xz;

            // Convert to polar coordinates
            float angle = atan2(pos.y, pos.x);
            float radius = length(pos);

            // Rotate the angle over time (swirl)
            angle += t * _Freq;

            // Prevent radius = 0 from breaking the funnel
            float safeRadius = max(radius, 0.001);

            // Inward suction
            float inward = safeRadius * _Amp;

            // Shrink radius (spiral inward)
            radius -= inward * 0.15;

            // Funnel depth (forces center downward)
            float funnelDepth = inward * 0.25 + (1.0 - saturate(radius * 2.0)) * _Amp;

            v.vertex.y -= funnelDepth;

            // Convert back to XZ
            float2 swirlPos;
            swirlPos.x = cos(angle) * radius;
            swirlPos.y = sin(angle) * radius;

            v.vertex.x = swirlPos.x;
            v.vertex.z = swirlPos.y;

            // swirl ripples
            float waveHeight = sin(angle * 4 + t * 2) * (_Amp * 0.2);
            v.vertex.y += waveHeight;

            o.vertColor = float3(1,1,1);
        }

        sampler2D _MainTex;


        void surf (Input IN, inout SurfaceOutput o)
        {
            float4 c = tex2D(_MainTex, IN.uv_MainTex);

            // Apply tint and vertex color
            float3 tinted = c.rgb * _Tint.rgb * IN.vertColor.rgb;

            o.Albedo = tinted;
        }

        ENDCG
    }

    FallBack "Diffuse"
}