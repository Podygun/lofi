                                                              
  
                                                                                      
                                                                                  
                                                                          
  
                                                                                 
                                
  
                                                                                       
                                                              
Shader "LofiRoom/Video Key"
{
    Properties
    {
        _VideoTex ("Видео (Render Texture)", 2D) = "black" {}
        [PerRendererData] _MainTex ("Спрайт-заглушка", 2D) = "white" {}
        _Color ("Оттенок", Color) = (1,1,1,1)

        [Toggle] _KeyEnabled ("Вырезать фон по цвету", Float) = 0
        [Toggle] _LumaKeyEnabled ("Убирать тёмный фон видео", Float) = 0
        _LumaThreshold ("Порог тёмного фона", Range(0,1)) = 0.27
        _LumaSoftness ("Мягкость порога", Range(0.001,0.5)) = 0.23
        _KeyColor ("Цвет фона", Color) = (0.086, 0.945, 0.102, 1)
        _Threshold ("Порог: сколько цвета считать фоном", Range(0,1)) = 0.30
        _Softness ("Мягкость края", Range(0.001,0.5)) = 0.12
        _Despill ("Убрать зелёный ободок", Range(0,1)) = 1.0
        _EdgeErode ("Сжать край внутрь", Range(0,0.9)) = 0.15
        _Brightness ("Яркость", Range(0,3)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Blend One OneMinusSrcAlpha                         
        ZWrite Off
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv         : TEXCOORD0;
            float4 color      : COLOR;
        };

        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float2 uv          : TEXCOORD0;
            float4 color       : COLOR;
        };

        TEXTURE2D(_VideoTex);
        SAMPLER(sampler_VideoTex);

        CBUFFER_START(UnityPerMaterial)
            float4 _VideoTex_ST;
            float4 _Color;
            float4 _KeyColor;
            float _KeyEnabled;
            float _LumaKeyEnabled;
            float _LumaThreshold;
            float _LumaSoftness;
            float _Threshold;
            float _Softness;
            float _Despill;
            float _EdgeErode;
            float _Brightness;
        CBUFFER_END

                                                                          
                                                                  
        half2 Chroma(half3 rgb)
        {
            half y = dot(rgb, half3(0.299, 0.587, 0.114));
            return half2(rgb.b - y, rgb.r - y);
        }

        Varyings vert (Attributes IN)
        {
            Varyings OUT;
            OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
            OUT.uv = TRANSFORM_TEX(IN.uv, _VideoTex);
            OUT.color = IN.color;
            return OUT;
        }

        half4 frag (Varyings IN) : SV_Target
        {
            half4 tex = SAMPLE_TEXTURE2D(_VideoTex, sampler_VideoTex, IN.uv);
            half3 rgb = tex.rgb;

            half2 pixelChroma = Chroma(rgb);
            half2 keyChroma = Chroma(_KeyColor.rgb);

                                                                
            half distance = length(pixelChroma - keyChroma);
            half keyed = smoothstep(_Threshold, _Threshold + _Softness, distance);

                                                                                    
                                                                                     
            keyed = saturate((keyed - _EdgeErode) / max(1e-4, 1.0 - _EdgeErode));

                                                                                
                                                                                
                                                                        
            half spillLimit = max(rgb.r, rgb.b);
            rgb.g = lerp(rgb.g, min(rgb.g, spillLimit), _Despill * _KeyEnabled);

            half luminance = dot(rgb, half3(0.299, 0.587, 0.114));
            half darkBackgroundAlpha = smoothstep(_LumaThreshold,
                                                  _LumaThreshold + _LumaSoftness,
                                                  luminance);
            half alpha = lerp(1.0, keyed, _KeyEnabled);
            alpha *= lerp(1.0, darkBackgroundAlpha, _LumaKeyEnabled);
            alpha *= _Color.a * IN.color.a;

            rgb = rgb * _Brightness * _Color.rgb * IN.color.rgb;

                                                                                     
                                                                                    
            return half4(rgb * alpha, alpha);
        }
        ENDHLSL

                                                                                     
        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

                                                        
        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
