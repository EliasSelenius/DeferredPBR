#version 330 core

#include "Engine.data.shaders.noise.glsl"

out vec4 FragColor;

in V2F {
    vec2 uv;
} v2f;

void main() {
    
    vec3 dir = normalize(vec3(v2f.uv * 2.0 - 1.0, 1.0));
    
    float n = noise(dir * 100.0);



    FragColor.a = 1.0;
    FragColor.rgb = vec3(n);
}