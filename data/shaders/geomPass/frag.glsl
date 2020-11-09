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
uniform sampler2D albedoMap;

in V2F {
    vec3 fragPos;
    vec3 normal;
    vec2 uv;
} v2f;

layout(location = 0) out vec4 g_Albedo_Metallic;
layout(location = 1) out vec4 g_Normal_Roughness;
layout(location = 2) out vec4 g_Fragpos;


void main() {
    g_Albedo_Metallic = vec4(material.albedo * texture(albedoMap, v2f.uv).rgb, material.metallic);
    g_Normal_Roughness = vec4(v2f.normal, material.roughness);
    g_Fragpos = vec4(v2f.fragPos, 1.0);
}