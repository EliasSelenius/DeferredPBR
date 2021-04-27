#version 330 core

/*
    unlit fragment shader

*/

in V2F {
    vec3 fragPos;
    vec3 normal;
    vec2 uv;
} v2f;

out vec4 FragColor;

void main() {
    FragColor = vec4(1, 0, 0, 1);
}