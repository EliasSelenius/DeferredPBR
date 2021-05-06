#version 330 core
#include "Engine.data.shaders.Camera.glsl"
#include "Engine.data.shaders.Model.glsl"

/*
    geomPass vertex shader

*/


layout (location = 0) in vec3 a_Pos;
layout (location = 1) in vec3 a_Normal;
layout (location = 2) in vec2 a_Uv;


out V2F {
    vec3 fragPos;
    vec3 normal;
    vec2 uv;
} v2f;

void main() {
    mat4 view_model = camera.view * model;
    vec4 pos = view_model * vec4(a_Pos, 1.0);
 
    v2f.fragPos = pos.xyz;
    v2f.normal = normalize((view_model * vec4(a_Normal, 0.0)).xyz);
    v2f.uv = a_Uv;
    
    gl_Position = camera.projection * pos;
}