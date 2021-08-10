#version 430 core

layout (local_size_x = 1) in;

struct Particle {
    vec4 pos;
    vec4 vel_size;
    vec4 color;
};

layout (std140, binding = 0) buffer ParitcleBuffer {
    Particle particles[];
};

float rand(float x) {
     return fract(sin(x)*100000.0);
}

bool outOfBounds(vec3 pos, float radius) {
    return dot(pos, pos) > radius * radius;
}

void main() {
    uint index = gl_GlobalInvocationID.x;
    particles[index].pos.xyz += particles[index].vel_size.xyz;

    if (outOfBounds(particles[index].pos.xyz, 20)) {
        // reset particle (as if a new spawned) 
        
        particles[index].pos.xyz = vec3(rand(float(index)) * 20.0, 0.0, 0.0);

    }
}
