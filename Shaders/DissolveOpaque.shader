Shader "Effects/DissolveOpaque"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0
        _Metallic ("Metallic", Range(0,1)) = 0
		[HDR]_Emission ("Emission", color) = (0,0,0)

		[Header(Dissolve)]
        _DissolveTex ("Dissolve Texture", 2D) = "black" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0.5

        [Header(Glow)]
        [HDR]_GlowColor("Color", Color) = (1, 1, 1, 1)
        _GlowRange("Range", Range(0, 0.3)) = 0.1
        _GlowFalloff("Falloff", Range(0.001, 0.3)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Geometry" }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;

		half _Smoothness;
        half _Metallic;
        half3 _Emission;

		sampler2D _DissolveTex;
        float _DissolveAmount;

        float3 _GlowColor;
        float _GlowRange;
        float _GlowFalloff;

        struct Input
        {
            float2 uv_MainTex;
			float2 uv_DissolveTex;
        };

        void surf (Input i, inout SurfaceOutputStandard o)
        {
			float dissolve = tex2D(_DissolveTex, i.uv_DissolveTex).r;
			dissolve = dissolve * 0.99999;
			float isVisible = dissolve - _DissolveAmount;
			clip(isVisible);
            
			float isGlowing = smoothstep(_GlowRange + _GlowFalloff, _GlowRange, isVisible);
			isGlowing *= 1 - pow(abs(_DissolveAmount - 1), 4);
			float3 glow = isGlowing * _GlowColor;

			// Albedo comes from a texture tinted by color
            fixed4 col = tex2D (_MainTex, i.uv_MainTex) * _Color;
            o.Albedo = col.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
			o.Emission = _Emission + glow;
        }
        ENDCG
    }
    FallBack "Standard"
}
