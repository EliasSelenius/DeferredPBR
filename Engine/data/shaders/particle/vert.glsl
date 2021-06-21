#version 330 core

#include "Engine.data.shaders.Camera.glsl"
#include "Engine.data.shaders.Window.glsl"

layout (location = 0) in vec3 a_Pos;
layout (location = 1) in vec3 a_Vel;
layout (location = 2) in float a_Size;


void main() {

    /*
        "vec4 eyePos=gl_ModelViewMatrix*vec4(pos,1.);"
        "vec4 projVoxel=gl_ProjectionMatrix*vec4(vec2(size),eyePos.zw);"
        "gl_PointSize=0.25/projVoxel.w*dot(ViewportSize,projVoxel.xy);"
        "gl_Position=gl_ProjectionMatrix*eyePos;"

        from: https://github.com/devoln/synthgen-particles-win/blob/master/src/shader.h
    */

    vec4 eyePos = camera.view * vec4(a_Pos, 1.0);

    vec4 projVox = camera.projection * vec4(vec2(a_Size), eyePos.zw);
    gl_PointSize = 0.25 / projVox.w * dot(window.size.xy, projVox.xy);
    
    gl_Position = camera.projection * eyePos;
}