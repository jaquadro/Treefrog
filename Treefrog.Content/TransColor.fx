// Effect dynamically changes color saturation.

sampler2D input : register(s0);

float4 transColor : register(C0);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 color = tex2D(input, uv.xy);

	if (abs(color.r - transColor.r) < 0.002 && abs(color.g - transColor.g) < 0.002 && abs(color.b - transColor.b) < 0.002) {
		color.a = 0;
	}

    return color;
}


technique TransColor
{
    pass Pass1
    {
		AlphaBlendEnable = TRUE;
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;
        PixelShader = compile ps_2_0 main();
    }
}
