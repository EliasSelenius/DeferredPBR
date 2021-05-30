#version 430 core

#include "Engine.data.shaders.noise.glsl"

layout (local_size_x = 1, local_size_y = 1) in;
layout (rgba8, binding = 0) uniform image2D outputImage;

layout (location = 0) uniform float time;

void main() {

    ivec2 coord = ivec2(gl_GlobalInvocationID.xy);

    float n = noise(vec3(coord, time * 10.0) / 10.0) * 0.5 + 0.5;

    imageStore(outputImage, coord, mix(vec4(1, 0, 0, 1), vec4(0, 0, 1, 1), n));
}