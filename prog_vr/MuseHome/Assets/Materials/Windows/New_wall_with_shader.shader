// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Unlit/New_wall_with_shader"
{
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
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
		Pass{
			SetTexture[_MainTex] {
				// Sets our color as the 'constant' variable
				constantColor[_Color]

				// Multiplies color (in constant) with texture
				combine constant * texture
			} 
		}
	}

}