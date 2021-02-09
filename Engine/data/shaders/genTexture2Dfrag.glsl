#version 330 core

#include "Engine.data.shaders.noise.glsl"

out vec4 FragColor;

in V2F {
    vec2 uv;
} v2f;

void main() {
    FragColor.a = 1.0;
    float n = noise(vec3(v2f.uv * 100.0, 0.0));
    FragColor.rgb = vec3(n);
}