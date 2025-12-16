#version 430

layout(location=0) in vec3 vColor;
out vec4 outputColor;

void main()
{
    vec2 uv = gl_PointCoord * 2.0 - 1.0; 
    float r = length(uv); 
    if (r > 1.0)
        discard;

    if (vColor.r == 2) {
        // mass marker
        if (r < 0.5) 
            outputColor = vec4(1.0, 0.0, 0.0, 1.0);
        else
            outputColor = vec4(1.0, 1.0, 1.0, 1);
    }
    else if (vColor.r == 3) {
        // init region marker
        float alfa = vColor.b;
        vec2 velocityDir = vec2(cos(-alfa),sin(-alfa));
        float velDist = length(uv-velocityDir);
        outputColor = velDist < 0.5 ? vec4(1.0, 0.0, 0.0, 0.7) : vec4(0.5, 0.5, 0.5, 0.5);
    } else {
        //actual particle
        float alpha = smoothstep(1.0, 0.0, r);
        alpha = alpha*alpha;
        outputColor = vec4(vColor*alpha, alpha);
    }

}