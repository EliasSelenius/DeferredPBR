#version 330 core

/*
    unlit fragment shader

*/

layout(std140) uniform Material {
    vec4 color;
};


in V2F {
    vec3 fragPos;
    vec3 normal;
    vec2 uv;
} v2f;

out vec4 FragColor;

void main() {
    FragColor = color;
}