#version 330 core

/*
    imagePass vertex shader

*/

layout(location = 0) in vec3 aPos;

out V2F {
    vec2 uv;
} v2f;

void main() {
    v2f.uv = (aPos.xy + vec2(1.0)) * 0.5;
    gl_Position = vec4(aPos, 1.0);
}