Shader "Unlit/Window_with_shader"
{
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_StencilMask("Stencil Mask", Range(0, 255)) = 1
	}
	SubShader{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry-1"}
		LOD 200

		//ColorMask 0
		Stencil{
			Ref[_StencilMask]
			Comp Never
			Fail Replace
		}
		Pass{}
	}

}