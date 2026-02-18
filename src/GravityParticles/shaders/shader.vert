#version 430

struct Particle
{
    vec2 position;
    vec2 velocity;
	vec2 prev_pos;
	vec2 prev_vel;
	vec2 prevprev_pos;
	vec2 prevprev_vel;
    float mass;
    float _pad0;
    vec4 color;
};

layout(std430, binding = 1) buffer OutputBuffer {
    Particle points[];
};

uniform mat4 projection;

layout(location=0) out vec3 vColor;

void main()
{
    uint id = gl_VertexID;
    gl_Position = projection * vec4(points[id].position, 0.0, 1.0);
    gl_PointSize = 15.0;
    if (points[id].color.r == 2) {
        gl_PointSize = 15.0;
    } else if (points[id].color.r == 3) {
        gl_PointSize = 30; //+points[id].color.g*300;
    }

    vColor = points[id].color.rgb; 
}