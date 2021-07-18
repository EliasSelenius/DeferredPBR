#version 430 core

layout (local_size_x = 1) in;

struct Particle {
    vec4 pos;
    vec4 vel_size;
};

layout (std140, binding = 0) buffer ParitcleBuffer {
    Particle particles[];
};


void main() {
    uint index = gl_GlobalInvocationID.x;
    particles[index].pos.xyz += particles[index].vel_size.xyz;
}
