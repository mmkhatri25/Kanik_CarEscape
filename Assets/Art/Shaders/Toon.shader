Shader "Roystan/Toon"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_RampTex("Ramp", 2D) = "white" {}
		// Ambient light is applied uniformly to all surfaces on the object.
		[HDR]
		_AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
		[HDR]
		_SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)

		[Space(35)]
		//Toggle Gloss
		[MaterialToggle] _toggleGloss ( "Gloss", Float ) = 0

		// Controls the size of the specular reflection.
		_Glossiness("Glossiness", Float) = 32

		[Space(35)]
		[HDR]
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0, 1)) = 0.716
		// Control how smoothly the rim blends when approaching unlit
		// parts of the surface.
		_RimThreshold("Rim Threshold", Range(0, 1)) = 0.1
		_RimPower("Rim Power", Range(0, 1)) = 0.1

		[Space(15)]
		[MaterialToggle] _toggleFrensel ( "Frensel", Float ) = 0
		_Frensel("FrenselPower", float) = 1

		[Space(25)]
		_minLightRange("minLightRange", Range(0,1)) = 0.136
		_MaxLightRange("MaxLightRange", Range(0,1)) = 0.896

		[Space(35)]
		// Outline
		[MaterialToggle] _ToggleBorderOutline("Outline", float) = 0
		[MaterialToggle] _ToggleDirectionalOutline("DirectionalOutline", float) = 0
		_OutlineExtrusion("Outline Extrusion", float) = 0
		_OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
		_OutlineDot("Outline Dot", float) = 0.25
		_HighlightScale("Highlight Scale", Range(0, 2)) = 0
	}
	SubShader
	{
		Pass
		{
			// Setup our pass to use Forward rendering, and only receive
			// data on the main directional light and ambient light.
			Tags
			{
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// Compile multiple versions of this shader depending on lighting settings.
			#pragma multi_compile_fwdbase
			
			#include "UnityCG.cginc"
			// Files below include macros and functions to assist
			// with lighting and shadows.
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;				
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : NORMAL;
				float2 uv : TEXCOORD0;
				float3 viewDir : TEXCOORD1;	
				// Macro found in Autolight.cginc. Declares a vector4
				// into the TEXCOORD2 semantic with varying precision 
				// depending on platform target.
				SHADOW_COORDS(2)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);		
				o.viewDir = WorldSpaceViewDir(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				// Defined in Autolight.cginc. Assigns the above shadow coordinate
				// by transforming the vertex from world space to shadow-map space.
				TRANSFER_SHADOW(o)
				return o;
			}
			
			float4 _Color;

			sampler2D _RampTex;

			float4 _AmbientColor;

			float4 _SpecularColor;
			float _Glossiness;		

			float4 _RimColor;
			float _RimAmount;
			float _RimThreshold;
			float _Frensel;
			float _toggleFrensel;
			float _RimPower;

			float _minLightRange;
			float _MaxLightRange;

			float _toggleGloss;

			float4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				float3 viewDir = normalize(i.viewDir);

				// Lighting below is calculated using Blinn-Phong,
				// with values thresholded to creat the "toon" look.
				// https://en.wikipedia.org/wiki/Blinn-Phong_shading_model

				// Calculate illumination from directional light.
				// _WorldSpaceLightPos0 is a vector pointing the OPPOSITE
				// direction of the main directional light.
				float NdotL = dot(_WorldSpaceLightPos0, normal);

				// Samples the shadow map, returning a value in the 0...1 range,
				// where 0 is in the shadow, and 1 is not.
				float shadow = SHADOW_ATTENUATION(i);
				// Partition the intensity into light and dark, smoothly interpolated
				// between the two to avoid a jagged break.

				float ramp = clamp(NdotL, _minLightRange, _MaxLightRange);
                float3 lighting = tex2D(_RampTex, float2(ramp, 0.5)).rgb;

				float lightIntensity = smoothstep(_minLightRange, _MaxLightRange, NdotL * shadow);
				// Multiply by the main directional light's intensity and color.
				float4 light = float4(lighting,0) * _LightColor0;

				// Calculate specular reflection.
				float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
				float NdotH = dot(normal, halfVector);
				// Multiply _Glossiness by itself to allow artist to use smaller
				// glossiness values in the inspector.
				float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
				float specularIntensitySmooth = specularIntensity;
				float4 specular = specularIntensitySmooth * _SpecularColor;

				// Calculate rim lighting.
				float rimDot = 1 - dot(viewDir, normal);
				float frensel = (_Frensel) - dot(viewDir, normal);
				// We only want rim to appear on the lit side of the surface,
				// so multiply it by NdotL, raised to a power to smoothly blend it.
				float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
				rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
				float4 rim = rimIntensity * _RimColor;
				rim *= _RimPower;

				float4 sample = tex2D(_MainTex, i.uv);
				
				frensel *= 0.2;
				return _Color * (light + _AmbientColor + specular) + rim + (frensel * _toggleFrensel);
				//return ((specular * _toggleGloss) + light + _AmbientColor + rim + (frensel * _toggleFrensel)) * _Color * sample;

			}
			ENDCG
		}

		// Outline pass
		Pass
		{
			// Won't draw where it sees ref value 4

			Cull front
			ZWrite OFF
			ZTest ON

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			// Properties
			uniform float4 _OutlineColor;
			uniform float _OutlineSize;
			uniform float _OutlineExtrusion;
			uniform float _OutlineDot;
			uniform float _HighlightScale;
			uniform float _ToggleBorderOutline;
			uniform float _ToggleDirectionalOutline;

			struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

			struct vertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				float4 newPos = input.vertex;

				float4 normal4 = float4(input.normal, 0.0);
				float3 normal = normalize(mul(normal4, unity_WorldToObject).xyz);
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

				float lightDot = saturate(dot(normal, lightDir));

				// outline lightdot = 1 , use directional outline = dirValue depends on the toggle
				_HighlightScale = lerp(0, 0.05, _HighlightScale);
				newPos += float4(input.normal, 0.0) * (_ToggleDirectionalOutline == 1 ? lightDot : 1.0) * _HighlightScale;

				output.pos = UnityObjectToClipPos(newPos);
				output.color = _OutlineColor;
				

				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				// checker value will be negative for 4x4 blocks of pixels
				// in a checkerboard pattern
				//input.pos.xy = floor(input.pos.xy * _OutlineDot) * 0.5;
				//float checker = -frac(input.pos.r + input.pos.g);

				// clip HLSL instruction stops rendering a pixel if value is negative
				//clip(checker);

				return input.color;
			}

			ENDCG
		}
		
		// Shadow casting support.
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"


	}

	
}