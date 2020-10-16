#version 330 core

/*
    lightPass vertex shader

*/

layout(location = 0) in vec3 aPos;

out vec2 uv;

void main() {
    uv = (aPos.xy + vec2(1.0)) * 0.5f;

    gl_Position = vec4(aPos, 1.0);
}