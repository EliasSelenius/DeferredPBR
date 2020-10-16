#version 330 core

/*
    lightPass fragment shader

*/

in vec2 uv;

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
    
    FragColor = vec4(albedo, 1.0);
}