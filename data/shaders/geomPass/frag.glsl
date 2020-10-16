#version 330 core

/*
    geomPass fragment shader

*/

struct Material {
    vec3 albedo;
    float metallic;
    float roughness;
};

uniform Material material;

in V2F {
    vec3 fragPos;
    vec3 normal;
    vec2 uv;
} v2f;

layout(location = 0) out vec4 g_Albedo_Metallic;
layout(location = 1) out vec4 g_Normal_Roughness;

void main() {
    g_Albedo_Metallic = vec4(material.albedo, material.metallic);
    g_Normal_Roughness = vec4(v2f.normal, material.roughness);
}