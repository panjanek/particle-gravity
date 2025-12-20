#version 430

uniform usampler2D plotTex;   // IMPORTANT: usampler2D
//uniform float invMaxValue;    // 1.0 / maxExpectedCount

in vec2 uv;
out vec4 outColor;

void main()
{
    

    ivec2 texSize = textureSize(plotTex, 0);
    ivec2 px = ivec2(uv * vec2(texSize));
    uint count = texelFetch(plotTex, px, 0).r;

    float exposure = 0.1;   // tune once
    float v = 1.0 - exp(-float(count) * exposure);
    v = pow(v, 0.7);
    outColor = vec4(v, v, v, 1.0);

    /*

    float invMaxValue = 1.0 / 30;
    float v = float(count) * invMaxValue;
    
    float maxValue = 100;
    float v = log(float(count) + 1.0) / log(maxValue + 1.0);

    v = clamp(v, 0.0, 1.0);
    outColor = vec4(v, v, v, 1.0);
    */

}