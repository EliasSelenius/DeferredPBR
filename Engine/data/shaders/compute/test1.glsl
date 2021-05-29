#version 430 core

#include "Engine.data.shaders.noise.glsl"

layout (local_size_x = 1, local_size_y = 1) in;

layout (rgba8, binding = 0) uniform image2D outputImage;

void main() {

    ivec2 coord = ivec2(gl_GlobalInvocationID.xy);

    float n = hash(vec3(coord, 0.0) / 100.0).x * 0.5 + 0.5;

    imageStore(outputImage, coord, vec4(1, 0, 0, 1) * n);
}