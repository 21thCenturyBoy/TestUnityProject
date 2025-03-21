// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
//AnimationEffects2D-->sakuraplus-->https://sakuraplus.github.io/make-terrain-with-google-elevation/index.html
Shader "TUT/HalftoneMask_ADV(B)05poprot" {
	Properties {
		_MainTex ("Texture A", 2D) = "black" {}
		_MainTexB ("Texture B", 2D) = "black" {}
		[Space(10)]
		_Position("Halftone Position", Float)=1
		_Diameter("Diameter", Range(0,1) )=0.25	
		_Num("Length",  Range(1,16)) = 3.0
		//_Rotation("Rotation",  Range(0,360)) = 0.0		
		_Rotation("Rotation",  Range(0,6.283)) = 0.0		
		_rotOffset("Offset Between Points",  Range(0,0.5)) = 0.0
	}


	SubShader {
		Tags {"Queue"="Transparent" }
		CGINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;  
		sampler2D _MainTexB;  

		uniform half4 _MainTex_ST;
		uniform half4 _MainTexB_ST;

		float _Diameter;
		float _Position;
		float _Num;
		float _rotOffset;
		float _Rotation;
			
		struct v2f {
			float4 pos : SV_POSITION;
			half2 uvTA: TEXCOORD0;
			half2 uvTB: TEXCOORD1;
			half2 uvORI: TEXCOORD2;//original
		};
		  
		v2f vert(appdata_img v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);

			o.uvTA=(v.texcoord-_MainTex_ST.zw)*_MainTex_ST.xy ;
			o.uvTB=(v.texcoord-_MainTexB_ST.zw)*_MainTexB_ST.xy ;
			o.uvORI.xy=v.texcoord;
					
			fixed2 offsetXY=fixed2(0.5,0.5);
		//	float Rot = _Rotation * (3.1415926f/180.0f);
			float Rot = _Rotation ;
			float s = sin(Rot);
			float c = cos(Rot);
			float2x2 rMatrix = float2x2(c, -s, s, c);
			rMatrix *= 0.5;
			rMatrix += 0.5;
			rMatrix = rMatrix * 2 - 1;
			o.uvORI.xy = mul(o.uvORI.xy-offsetXY, rMatrix)+offsetXY;
			
			return o;
		}



		fixed4 frag(v2f i) : SV_Target {
			float _rd;
			fixed2 posCenter;	
			fixed diameterW,diameterH;
			diameterW=_Diameter*(1-_rotOffset/2);//width of grid , reduce when _rotOffset is larger than zero 		
			diameterH=_Diameter;
			
			fixed indexOfGrid=floor((i.uvORI.x)/diameterW);//num of grids between uv and PosW			
			
			//if(i.uvORI.x>=_Position){
			//			return tex2D(_MainTexB, i.uvTB).rgba ;//return texture A when uv is in front (larger) of PosW
			//}

			posCenter.x=(indexOfGrid+0.5)*diameterW;					
			fixed modOffset=frac(indexOfGrid*_rotOffset)*_Diameter;
			posCenter.y=(floor((i.uvORI.y-modOffset)/diameterH)+ 0.5)*diameterH+modOffset;							

			_rd=0.5*(_Position-posCenter.x)/_Num;//radius of the current grid 

			float _rdNext=_rd+0.5*diameterW/_Num;		
			fixed2 posCenterNextUp=posCenter-fixed2(diameterW,_Diameter*(_rotOffset-1));
			fixed2 posCenterNextDown=posCenter-fixed2(diameterW,_Diameter*_rotOffset);	//center of down-next grid		
			float _rdPrev=_rd-0.5*diameterW/_Num;
			fixed2 posCenterPrevUp=posCenter+fixed2(diameterW,_Diameter*(_rotOffset-1));
			fixed2 posCenterPrevDown=posCenter+fixed2(diameterW,_Diameter*_rotOffset);	//center of down-next grid		
			
			//fixed inCircle=step(abs(i.uvORI.x-posCenter.x),_rd)*step(abs(i.uvORI.y-posCenter.y),_rd);	  //Square
			fixed inCircle=step(distance(i.uvORI,posCenter),_rd);
			inCircle+=step(distance(i.uvORI,posCenterNextUp),_rdNext)+step(distance(i.uvORI,posCenterNextDown),_rdNext);	
			inCircle+=step(distance(i.uvORI,posCenterPrevUp),_rdPrev)+step(distance(i.uvORI,posCenterPrevDown),_rdPrev);	
			inCircle=saturate(inCircle);

			fixed4 texA=tex2D(_MainTex, i.uvTA).rgba;
			fixed4 texB=tex2D(_MainTexB, i.uvTB).rgba;
			fixed4 sum= lerp(texB,texA,inCircle);
			
			return sum;		
		}
		    
		ENDCG


		Pass {

			//ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			  
			#pragma vertex vert  
			#pragma fragment frag		
			#pragma target 3.0
			  
			ENDCG  
		}

	} 
	FallBack "Diffuse"
}