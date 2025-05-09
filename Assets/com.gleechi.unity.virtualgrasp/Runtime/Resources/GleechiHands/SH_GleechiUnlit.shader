Shader "Gleechi/Unlit"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {} // Eliminado porque no se usará la textura
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;        
                UNITY_VERTEX_INPUT_INSTANCE_ID       
            };

            struct v2f
            {  
                float4 vertex : SV_POSITION;                
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); 
    
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                // Gris oscuro (por ejemplo, RGB 0.2, 0.2, 0.2)
                return fixed4(0.01, 0.01, 0.01, 1.0);
            }
            ENDCG
        }
    }
}
