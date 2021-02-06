#version 330 core
#include "Engine.data.shaders.Camera.glsl"

/*
    cubemapSkybox vertex shader

*/


layout (location = 0) in vec3 a_Pos;

out vec3 texCoords;

void main() {
    texCoords = a_Pos;

    mat4 rotview = mat4(mat3(camera.view)); // remove translation from view matrix
    vec4 pos = camera.projection * rotview * vec4(a_Pos, 1.0);
    gl_Position = pos.xyww;
}