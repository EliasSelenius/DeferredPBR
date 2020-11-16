#version 330 core
#include "Camera.glsl"
#include "PBR.glsl"


/*
    lightPass_pointlight fragment shader

*/


in V2F {
    vec2 uv;
} v2f;


uniform vec3 lightPosition;
uniform vec3 lightColor;

uniform sampler2D g_Albedo_Metallic;
uniform sampler2D g_Normal_Roughness;
uniform sampler2D g_Fragpos;


out vec4 FragColor;


void main() {
    vec4 gam = texture(g_Albedo_Metallic, v2f.uv);
    vec4 gnr = texture(g_Normal_Roughness, v2f.uv);
    vec4 gf  = texture(g_Fragpos, v2f.uv); 

    vec3 albedo = gam.xyz;
    float metallic = gam.w;
    vec3 normal = gnr.xyz;
    float roughness = gnr.w;
    vec3 fragpos = gf.xyz;
    
    // lp: light position in view space
    vec3 lp = (view * vec4(lightPosition, 1.0)).xyz;

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);

    vec3 V = normalize(-fragpos);

    vec3 light = CalcPointlight(lp, lightColor, fragpos, F0, normal, V, albedo, roughness, metallic);

    //vec3 light = vec3(0.1);

    // ambient  
    //light += albedo * 0.1;

    FragColor = vec4(light, 1.0);
}