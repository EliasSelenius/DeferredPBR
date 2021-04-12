#version 330 core
#include "Engine.data.shaders.Camera.glsl"

/*
    gizmo vertex shader

*/

layout(location = 0) in vec3 a_Pos;
layout(location = 1) in vec4 a_Color;

out vec4 color;

void main() {
    gl_Position = camera.projection * camera.view * vec4(a_Pos, 1.0);
    color = a_Color;
}