#version 330 core
#include "Camera.glsl"

/*
    lightPass_pointlight vertex shader

*/

layout(location = 0) in vec3 aPos;

uniform mat4 model;


void main() {
    gl_Position = camera.projection * camera.view * model * vec4(aPos, 1.0);
}