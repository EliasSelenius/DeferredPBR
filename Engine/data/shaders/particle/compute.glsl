#version 430 core

#include "Engine.data.shaders.Application.glsl"

layout (local_size_x = 1) in;

struct Particle {
    vec4 pos;
    vec4 vel_size;
    vec4 color;
};

layout (std140, binding = 0) buffer ParitcleBuffer {
    Particle particles[];
};

uniform vec3 origin;
uniform float maxLifeTime = 4.0;

float rand(float x) {
     return fract(sin(x)*100000.0);
}

bool outOfBounds(vec3 pos, float radius) {
    return distance(pos, origin) > radius;
}

vec3 randDirection(float x) {
    vec3 r = vec3(rand(x)-0.5, rand(x + 1)-0.5, rand(x + 2)-0.5);
    return normalize(r) * rand(x + 3);
}

void main() {
    
    float deltatime = getDeltatime();
    
    uint index = gl_GlobalInvocationID.x;
    Particle particle = particles[index];
    
    // update particle position by velocity
    particle.pos.xyz += particle.vel_size.xyz * deltatime;


    if (outOfBounds(particle.pos.xyz, 20)) {
        // reset particle (as if a new spawned) 
        
        float seed = float(index);// * getTime();
        particle.pos.xyz = origin + randDirection(seed) * 19.0;


        particle.color = vec4(1.0, 0.0, 0.0, 1.0);
        //particle.vel_size.xyz = vec3(0.0);
    }

    particles[index] = particle;
}
