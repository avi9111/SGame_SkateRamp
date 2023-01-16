
Shader "GameKit/Chapter8_AlphaTest" {

	Properties{

		_Color("Color",Color) = (1,1,1,1)

		_MainTex("MainTex",2D) = "white"{}

		_Cutoff("Alpha Cutoff",Range(0,1)) = 0.5  //在材质面板显示和调节透明度测试的控制阈值

	}

		SubShader{

			Tags{"Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout"}

			//通常，使用透明度测试的Shader都应该在SubShader中设置这三个标签

			Pass{

				Tags{"LightMode" = "ForwardBase"}

				CGPROGRAM

				#pragma vertex vert

				#pragma fragment frag

				#include "Lighting.cginc"



				fixed4 _Color;

				sampler2D _MainTex;

				float4 _MainTex_ST;

				fixed _Cutoff;



				struct a2v {

					float4 vertex:POSITION;

					//float3 normal:NORMAL;
					float4 texcoord:TEXCOORD0;
				};



				struct v2f {

					float4 pos:SV_POSITION;

					//float3 worldNormal:TEXCOORD0;

					//float3 worldPos:TEXCOORD1;

					float2 uv:TEXCOORD2;

				};



				v2f vert(a2v v) {

					v2f o;

					o.pos = UnityObjectToClipPos(v.vertex);

					//o.worldNormal = UnityObjectToWorldNormal(v.normal);

					//o.worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;

					o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);



					return o;

				}



				fixed4 frag(v2f i) :SV_Target{

					//fixed3 worldNormal = normalize(i.worldNormal);

					//fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));



					fixed4 texColor = tex2D(_MainTex,i.uv);



					clip(texColor.a - _Cutoff);
					return texColor;
					////clip函数做透明度的比较后进行裁剪操作

					//fixed3 albedo = texColor.rgb*_Color;

					//fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz*albedo;

					//fixed3 diffuse = _LightColor0.rgb*albedo*max(0,dot(worldNormal,worldLightDir));



					//return fixed4(ambient + diffuse,1.0);



				}

				ENDCG

			}

		}

			FallBack "Diffuse"

}
