Shader "Custom/RingGlowShader"
{
    Properties
    {
        _MainColor ("Glow Color", Color) = (1, 0.8, 0.3, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 5
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 3
        _NoiseScale ("Noise Scale", Float) = 5
        _NoiseSpeed ("Noise Speed", Float) = 0.5
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.3
        _GradientHeight ("Gradient Height", Float) = 2
        _GradientPower ("Gradient Power", Range(0.1, 5)) = 2
        _BottomIntensity ("Bottom Intensity", Range(0, 5)) = 2
        _GradientOffset ("Gradient Offset", Float) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha One // Additive blending
            ZWrite Off
            Cull Off // แสดงทั้งสองด้าน
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : NORMAL;
                float3 viewDirWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor;
                float _GlowIntensity;
                float _FresnelPower;
                float _NoiseScale;
                float _NoiseSpeed;
                float _NoiseStrength;
                float _GradientHeight;
                float _GradientPower;
                float _BottomIntensity;
                float _GradientOffset;
            CBUFFER_END
            
            // Simple Noise Function
            float noise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            float smoothNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            // Convert UV to Polar Coordinates
            float2 toPolar(float2 uv)
            {
                float2 centered = uv - 0.5;
                float angle = atan2(centered.y, centered.x);
                float radius = length(centered) * 2.0;
                return float2(angle / (2.0 * 3.14159265359), radius);
            }
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);
                
                OUT.positionCS = positionInputs.positionCS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                OUT.uv = IN.uv;
                OUT.positionWS = positionInputs.positionWS;
                
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                // Normalize vectors
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(IN.viewDirWS);
                
                // Fresnel Effect
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                
                // Polar Coordinates for noise
                float2 polarUV = toPolar(IN.uv);
                
                // Animated Noise
                float2 noiseUV = polarUV * _NoiseScale + float2(_Time.y * _NoiseSpeed, 0);
                float noiseValue = smoothNoise(noiseUV);
                noiseValue = lerp(1.0, noiseValue, _NoiseStrength);
                
                // Vertical Gradient ใช้ World Space Y (ไม่งอตาม mesh)
                float worldHeight = IN.positionWS.y;
                
                // คำนวณ gradient จากล่างขึ้นบน
                float normalizedHeight = saturate((worldHeight - _GradientOffset) / _GradientHeight);
                
                // Inverse gradient (ล่างสว่าง บนมด)
                float verticalGradient = pow(1.0 - normalizedHeight, _GradientPower);
                
                // เพิ่มความสว่างที่ด้านล่าง
                float bottomGlow = pow(verticalGradient, 0.5) * _BottomIntensity;
                
                // Combine effects
                float glow = fresnel * noiseValue * (verticalGradient + bottomGlow);
                
                // Final color with HDR intensity
                float3 finalColor = _MainColor.rgb * glow * _GlowIntensity;
                float alpha = glow;
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}