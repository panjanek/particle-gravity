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

    float exposure = 0.05;   // tune once
    float v = 1.0 - exp(-float(count) * exposure);
    v = pow(v, 0.7);

    outColor = vec4(v, v, v, 1.0);
}