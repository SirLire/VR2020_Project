Shader "Unlit/Wall_with_shader"
{
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_StencilMask("Stencil Mask", Range(0, 255)) = 1
	}
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		Stencil{
			Ref[_StencilMask]
			Comp NotEqual
			Pass Zero
		}
		Pass{}
	}

}
