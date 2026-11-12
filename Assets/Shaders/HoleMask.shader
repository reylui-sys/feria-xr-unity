Shader "ARTEC_IM/IMDepthHoleMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-10" }

        Pass
        {
            // This shader is used to mark pixels as holes in the stencil buffer, so they are not rendered with other shaders

            ColorMask 0
            ZWrite Off

            // Stencil buffer used to mark pixels as holes

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }
        }
    }
}