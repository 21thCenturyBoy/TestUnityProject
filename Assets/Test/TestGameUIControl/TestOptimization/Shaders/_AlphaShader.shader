Shader "TestOptimization/ZTestAlphaShader"
{
  SubShader{
       ZWrite on 
       ZTest Always

        Pass{
        
        Color(1,1,1,1)
        }
    
    }
}