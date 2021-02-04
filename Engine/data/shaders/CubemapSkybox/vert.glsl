#version 330 core
#include "Engine.data.shaders.Camera.glsl"

/*
    cubemapSkybox vertex shader

*/


layout (location = 0) in vec3 a_Pos;

out vec3 texCoords;

void main() {
    vec4 pos = camera.projection * mat4(mat3(camera.view)) * vec4(a_Pos, 1.0);
    gl_Position = pos.xyww;

    texCoords = a_Pos;
}