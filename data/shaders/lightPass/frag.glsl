#version 330 core

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
    
    vec3 ld = (view * vec4(lightDir, 0.0)).xyz;
    vec3 color = albedo * lightColor;
    vec3 light = color * 0.1;
    light += color * max(0, dot(normal, ld));

    FragColor = vec4(light, 1.0);
}