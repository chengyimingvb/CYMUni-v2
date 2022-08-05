Shader "Custom/Territory/UnlitArea" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,1)
	_Alpha ("Alpha", Range (0, 1)) = 1
}
 
SubShader {
    Tags {
      "Queue"="Geometry+151"
      "RenderType"="Transparent"
  	}
  	Blend SrcAlpha OneMinusSrcAlpha
  	ZTest Always
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag	
		#include "UnityCG.cginc"			

		fixed4 _Color;
		fixed _Alpha;

		struct AppData {
			float4 vertex : POSITION;
		};

		struct VertexToFragment {
			fixed4 pos : SV_POSITION;	
		};

		//Vertex shader
		VertexToFragment vert(AppData v) {
			VertexToFragment o;							
			o.pos = UnityObjectToClipPos(v.vertex);
			return o;									
		}
		
		fixed4 frag(VertexToFragment i) : SV_Target {
			_Color.a = _Alpha;
			return _Color;
		}
			
		ENDCG
    }
    }
}
