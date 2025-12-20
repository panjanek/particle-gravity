#version 430

uniform sampler2D plotTex;

in vec2 uv;
out vec4 outColor;

void main()
{
    outColor = texture(plotTex, uv);
}