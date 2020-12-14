#version 330 core
#include "PBR.glsl"
#include "GBuffer.glsl"
#include "Window.glsl" 

/*
    lightPass_pointlight fragment shader

*/


uniform vec3 lightPosition;
uniform vec3 lightColor;

out vec4 FragColor;

void main() {

    vec2 uv = gl_FragCoord.xy / window.size.xy;

    GBufferData fragdata;
    readGBuffer(uv, fragdata);    

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, fragdata.albedo, fragdata.metallic);

    vec3 V = normalize(-fragdata.fragpos);

    vec3 light = CalcPointlight(lightPosition, lightColor, fragdata.fragpos, F0, fragdata.normal, V, fragdata.albedo, fragdata.roughness, fragdata.metallic);
    light = max(light, vec3(0.0));

    FragColor = vec4(light, 1.0);
}