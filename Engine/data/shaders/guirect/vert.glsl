#version 330 core
#include "Engine.data.shaders.Camera.glsl"


/*
    text vertex shader

*/


layout (location = 0) in vec2 a_Pos;
layout (location = 1) in vec2 a_Uv;
layout (location = 2) in vec4 a_Color;


out V2F {
    vec2 uv;
    vec4 color;
} v2f;

void main() {

    v2f.uv = a_Uv;
    v2f.color = a_Color;

    gl_Position = camera.projection * vec4(a_Pos.xy, 0, 1.0);
}