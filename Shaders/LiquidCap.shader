Shader "Effects/LiquidCap"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
		[IntRange] _StencilRef ("Stencil Reference Value", Range(0, 255)) = 0
    }
    
	SubShader
    {
        Tags { "Queue" = "Geometry-1" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" }

		Pass {
			Stencil {
				Ref [_StencilRef]
				Comp Equal
			}	
			CGPROGRAM
				#include "UnityCG.cginc"
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0

				fixed4 _Color;

				struct appdata{
					float4 vertex : POSITION;
				};

				struct v2f{
					float4 vertex : SV_POSITION;
				};

				v2f vert(appdata v){
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_TARGET{
					return _Color;
				}
			ENDCG
		}
    }
    FallBack "Standard"
}
