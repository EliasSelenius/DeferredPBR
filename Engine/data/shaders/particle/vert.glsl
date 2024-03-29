#version 330 core

#include "Engine.data.shaders.Camera.glsl"
#include "Engine.data.shaders.Application.glsl"

layout (location = 0) in vec4 a_Pos;
layout (location = 1) in vec4 a_Vel_Size;
layout (location = 2) in vec4 a_Color;

out V2F {
    vec4 color;
} v2f;

void main() {

    /*
        "vec4 eyePos=gl_ModelViewMatrix*vec4(pos,1.);"
        "vec4 projVoxel=gl_ProjectionMatrix*vec4(vec2(size),eyePos.zw);"
        "gl_PointSize=0.25/projVoxel.w*dot(ViewportSize,projVoxel.xy);"
        "gl_Position=gl_ProjectionMatrix*eyePos;"

        from: https://github.com/devoln/synthgen-particles-win/blob/master/src/shader.h
    */

    vec4 eyePos = camera.view * vec4(a_Pos.xyz, 1.0);

    vec2 windowSize = getWindowSize();
    vec4 projVox = camera.projection * vec4(vec2(a_Vel_Size.w), eyePos.zw);
    gl_PointSize = 0.25 / projVox.w * dot(windowSize, projVox.xy);
    
    gl_Position = camera.projection * eyePos;

    v2f.color = a_Color;
}