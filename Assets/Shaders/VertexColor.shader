Shader "Custom/VertexColor" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		PASS {
			//开启光照，也就是定义材质块中的设定是否有效
			Lighting On

			//ColorMaterial：使用每个顶点的颜色替代材质中的颜色集。AmbientAndDiffuse替代材质的阴影光和漫反射值；Emission替代材质中的光发射值
			ColorMaterial AmbientAndDiffuse

			//用SetTexture命令来设置纹理，可以定义影像纹理如何混合、组合以及如何运用于我们的彩现环境里
			//当使用片段编程时候SetTexture命令无效，命令必须处在Pass的末尾。
			SetTexture [_MainTex] {

				//combine src1 * src2
				//src1和src2相乘，结果纹理将比输入纹理更暗
				//所有的src属性可以是previous, constant, primary或texture
				//这里的texture来源是当前的纹理(_MainTex)，它将与主要的颜色互相搭配(*)，主色为照明设备的颜色，它是由Material计算出来的结果
				combine texture * primary DOUBLE
			}
		}
	}
	FallBack "Diffuse"
}
