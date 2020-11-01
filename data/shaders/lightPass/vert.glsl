#version 330 core

/*
    lightPass vertex shader

*/

layout(location = 0) in vec3 aPos;

out V2F {
    vec2 uv;
    vec3 viewray;
} v2f;


uniform mat4 view;


void main() {
    v2f.uv = (aPos.xy + vec2(1.0)) * 0.5f;

    vec3 posVS = (view * vec4(aPos, 1.0)).xyz;
    v2f.viewray = vec3(posVS.xy / posVS.z, 1.0);

    gl_Position = vec4(aPos, 1.0);
}