#version 430

uniform sampler2D plotTex;

in vec2 uv;
out vec4 outColor;

void main()
{
    outColor = texture(plotTex, uv);
    /*
    ivec2 texSize = textureSize(plotTex, 0);
    ivec2 px = ivec2(uv * vec2(texSize));
    vec4 color = texelFetch(plotTex, px, 0);
    outColor = color;
    */

    /*
    vec2 texel = 1.0 / textureSize(plotTex, 0);
    vec4 c =
      texture(plotTex, uv) +
      texture(plotTex, uv + vec2(texel.x, 0)) +
      texture(plotTex, uv + vec2(0, texel.y)) +
      texture(plotTex, uv + texel);

    outColor = c * 0.25;*/
}