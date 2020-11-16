#version 330 core
#include "Camera.glsl"
#include "PBR.glsl"

/*
    lightPass_dirlight fragment shader

*/


in V2F {
    vec2 uv;
    vec3 viewray;
} v2f;


uniform vec3 lightDir;
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
    
    // ld: light direction in view space
    vec3 ld = (view * vec4(lightDir, 0.0)).xyz;

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);

    vec3 V = normalize(-fragpos);

    /*
    vec3 ld = (view * vec4(lightDir, 0.0)).xyz;
    vec3 color = albedo * lightColor;
    vec3 light = color * 0.1;
    light += color * max(0, dot(normal, ld));
    */


    /*
    // Calculate our projection constants (you should of course do this in the app code, I'm just showing how to do it)
    ProjectionA = FarClipDistance / (FarClipDistance - NearClipDistance);
    ProjectionB = (-FarClipDistance * NearClipDistance) / (FarClipDistance - NearClipDistance);

    // Sample the depth and convert to linear view space Z (assume it gets sampled as
    // a floating point value of the range [0,1])
    float depth = DepthTexture.Sample(PointSampler, texCoord).x;
    float linearDepth = ProjectionB / (depth - ProjectionA);
    float3 positionVS = viewRay * linearDepth;
    */

    // calc fragpos
    /*float far = 10000.0;
    float near = 0.1;
    float pa = far / (far - near);
    float pb = (-far * near) / (far - near);
    float ldepth = pb / (gl_FragCoord.z - pa);
    vec3 fragpos = v2f.viewray * ldepth;
    */

    vec3 light = CalcDirlight(ld, lightColor, F0, normal, V, albedo, roughness, metallic);

    // ambient  
    light += albedo * 0.1;

    FragColor = vec4(light, 1.0);
}