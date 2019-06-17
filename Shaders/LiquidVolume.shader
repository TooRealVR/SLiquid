Shader "Effects/LiquidVolume"
{
    Properties
    {
		_Color ("Color", Color) = (1, 1, 1, 1)
		[IntRange] _StencilRef ("Stencil Reference Value", Range(0, 255)) = 0
    }
    
	SubShader
    {        
		Tags { "Queue" = "Geometry-2" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" }

		Pass {
			Cull Front
			Stencil {
				Ref [_StencilRef]
				Comp Always
				Pass Replace
			}
			CGPROGRAM
				#include "UnityCG.cginc"
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0

				fixed4 _Color;
				float4 _Plane;

				struct appdata{
					float4 vertex : POSITION;
				};

				struct v2f{
					float4 vertex : SV_POSITION;
					float3 worldPos : TEXCOORD0;
				};

				v2f vert(appdata v){
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.worldPos = mul(unity_ObjectToWorld, v.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_TARGET{
					// Calculate signed distance to plane
					float distance = dot(i.worldPos, _Plane.xyz) + _Plane.w;
					// Discard surface above plane
					clip(-distance);	
				
					return _Color;
				}
			ENDCG
		}

		Pass {
			Cull Back
			Stencil {
				Ref 0
				Comp Always
				Pass Replace
			}
			CGPROGRAM
				#include "UnityCG.cginc"
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0

				fixed4 _Color;
				float4 _Plane;

				struct appdata{
					float4 vertex : POSITION;
				};

				struct v2f{
					float4 vertex : SV_POSITION;
					float3 worldPos : TEXCOORD0;
				};

				v2f vert(appdata v){
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.worldPos = mul(unity_ObjectToWorld, v.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_TARGET{
					// Calculate signed distance to plane
					float distance = dot(i.worldPos, _Plane.xyz) + _Plane.w;
					// Discard surface above plane
					clip(-distance);	
				
					return _Color;
				}
			ENDCG
		}
    }
    FallBack "Standard"
}
