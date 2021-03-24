#version 330 core

#include "Engine.data.shaders.noise.glsl"

out vec4 FragColor;

in V2F {
    vec2 uv;
} v2f;


uniform int cubemapSide;

void main() {
    vec2 nuv = v2f.uv * 2.0 - 1.0;

    vec3 dir;
    
    switch (cubemapSide) {
        case 0: dir = vec3(1.0, -nuv.yx); break;
        case 1: dir = vec3(-1.0, -nuv.y, nuv.x); break;

        case 2: dir = vec3(nuv.x, 1.0, nuv.y); break;
        case 3: dir = vec3(nuv.x, -1.0, -nuv.y); break;

        case 4: dir = vec3(nuv.x, -nuv.y, 1.0); break;
        case 5: dir = vec3(-nuv, -1.0); break;

        default: dir = vec3(0.0); break;
    }

    dir = normalize(dir);

    float n = noise(dir * 50.0);


/*
    if (length(nuv) > 0.9) {
        color = vec3(nuv, 0.0);
    }
*/

    FragColor.a = 1.0;
    FragColor.rgb = (dir*0.5+0.5) * n;
}