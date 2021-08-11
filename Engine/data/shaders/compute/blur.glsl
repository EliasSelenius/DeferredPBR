#version 430 core

#include "Engine.data.shaders.Application.glsl"

layout (local_size_x = 1, local_size_y = 1) in;

layout (binding = 0) uniform sampler2D srcImg;
writeonly layout (binding = 1) uniform image2D destImg;


uniform float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

void main() {
    ivec2 coord = ivec2(gl_GlobalInvocationID.xy);

    vec2 wSize = getWindowSize() / 2.0;

    //vec3 res = imageLoad(srcImg, coord).rgb * weight[0];
    vec2 uv = gl_GlobalInvocationID.xy / wSize;
    vec3 res = texture(srcImg, uv).rgb * weight[0];

    for (int i = 1; i < 5; i++) {
        //res += imageLoad(srcImg, coord + ivec2(i, 0)).rgb * weight[i];
        //res += imageLoad(srcImg, coord - ivec2(i, 0)).rgb * weight[i];

        vec2 ofs = vec2(i, 0.0);
        res += texture(srcImg, (gl_GlobalInvocationID.xy + ofs) / wSize).rgb * weight[i];
        res += texture(srcImg, (gl_GlobalInvocationID.xy - ofs) / wSize).rgb * weight[i];
    }


    imageStore(destImg, coord, vec4(res, 1.0));
}