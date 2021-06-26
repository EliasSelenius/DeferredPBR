#version 430 core

layout (local_size_x = 1, local_size_y = 1) in;

readonly layout (binding = 0, rgba16f) uniform image2D srcImg;
writeonly layout (binding = 1) uniform image2D destImg;


uniform float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

void main() {
    ivec2 coord = ivec2(gl_GlobalInvocationID.xy);

    vec3 res = imageLoad(srcImg, coord).rgb * weight[0];

    for (int i = 1; i < 5; i++) {
        res += imageLoad(srcImg, coord + ivec2(i, 0)).rgb * weight[i];
        res += imageLoad(srcImg, coord - ivec2(i, 0)).rgb * weight[i];
    }


    imageStore(destImg, coord, vec4(res, 1.0));
}