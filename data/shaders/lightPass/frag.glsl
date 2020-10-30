#version 330 core
#include "PBR.glsl"


/*
    lightPass fragment shader

*/

in vec2 uv;

uniform mat4 view;
uniform vec3 lightDir;
uniform vec3 lightColor;

uniform sampler2D g_Albedo_Metallic;
uniform sampler2D g_Normal_Roughness;

out vec4 FragColor;


void main() {
    vec4 gam = texture(g_Albedo_Metallic, uv);
    vec4 gnr = texture(g_Normal_Roughness, uv);
    
    vec3 albedo = gam.xyz;
    float metallic = gam.w;
    vec3 normal = gnr.xyz;
    float roughness = gnr.w;
    
    // ld: light direction in view space
    vec3 ld = (view * vec4(lightDir, 0.0)).xyz;

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);

    /*
    vec3 ld = (view * vec4(lightDir, 0.0)).xyz;
    vec3 color = albedo * lightColor;
    vec3 light = color * 0.1;
    light += color * max(0, dot(normal, ld));
    */

    vec3 light = CalcDirlight(ld, lightColor, F0, normal, vec3(0.0), albedo, roughness, metallic);

    // ambient
    light += albedo * 0.1;

    FragColor = vec4(light, 1.0);
}