Shader "CameraShader/Cameramap"
{
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		Cull Back
		LOD 100

		CGINCLUDE
#include "UnityCG.cginc"

		uniform float4x4 _ProjMatrix;
	uniform float4x4 _WorldToCam;
	sampler2D _RtCamera;

	struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float2 texUV : TEXCOORD2;
	};

	struct v2f
	{
		float4 vertex : SV_POSITION;
		float3 worldPos : TEXCOORD0;
		float3 normal : TEXCOORD1;
	};

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = float4(v.texUV.x * 2.0 - 1.0, v.texUV.y * -2.0 + 1.0, 0, 1);
		o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
		o.normal = UnityObjectToWorldNormal(v.normal);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		half4 relativeToCam = mul(_WorldToCam, half4(i.worldPos, 1.0));
		half4 projPos = mul(_ProjMatrix, relativeToCam);
		projPos.z *= -1;
		float2 projUV = projPos.xy / projPos.z;
		projUV = projUV * 0.5 + 0.5;
		float4 col = tex2D(_RtCamera, projUV);
		float edge = 0 < projUV.x && projUV.x < 1 && 0 < projUV.y && projUV.y < 1 && 0 < projPos.z;
		return col * edge;
	}

		ENDCG

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
			ENDCG
	}
	}
}
