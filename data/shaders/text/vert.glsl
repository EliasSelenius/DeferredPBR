#version 330 core
#include "Camera.glsl"


/*
    text vertex shader

*/

uniform vec2 size;
uniform vec2 pos;

layout (location = 0) in vec3 a_Pos;
layout (location = 1) in vec2 a_Uv;

out V2F {
    vec2 uv;
} v2f;

void main() {

    v2f.uv = a_Uv;

    gl_Position = camera.projection * vec4(pos + a_Pos.xy * size, 0, 1.0);
}