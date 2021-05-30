#version 430 core


layout (local_size_x = 1, local_size_y = 1) in;
layout (r8, binding = 0) uniform image2D outputImage;

layout (location = 0) uniform float time;

int adj(ivec2 ofs) {
    return int(step(0.5, imageLoad(outputImage, ivec2(gl_GlobalInvocationID.xy) + ofs).r));
}

void main() {


    //float n = noise(vec3(coord, time * 10.0) / 10.0) * 0.5 + 0.5;

    //n = smoothstep(0.4, 0.5, n);

    int t = adj(ivec2(-1, -1))
          + adj(ivec2(-1,  0))
          + adj(ivec2(-1, 1))
          + adj(ivec2(0, -1))
          + adj(ivec2(0, 1))
          + adj(ivec2(1, -1))
          + adj(ivec2(1, 0))
          + adj(ivec2(1, 1));

    int th = adj(ivec2(0));

    int cellout = 0;


    if (th == 1) {
        cellout = t == 3 || t == 2 ? 1 : 0;
    } else {
        if (t == 3) cellout = 1;
    }

    imageStore(outputImage, ivec2(gl_GlobalInvocationID.xy), vec4(cellout));
}