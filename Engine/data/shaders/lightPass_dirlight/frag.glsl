#version 330 core
#include "Engine.data.shaders.Camera.glsl"
#include "Engine.data.shaders.PBR.glsl"
#include "Engine.data.shaders.GBuffer.glsl"

/*
    lightPass_dirlight fragment shader

*/


in V2F {
    vec2 uv;
    vec3 viewray;
} v2f;


uniform vec3 lightDir;
uniform vec3 lightColor;
uniform float ambientScale;

out vec4 FragColor;


void main() {
 
    GBufferData fragdata;
    readGBuffer(v2f.uv, fragdata);
    
    // ld: light direction in view space
    vec3 ld = (camera.view * vec4(lightDir, 0.0)).xyz;

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, fragdata.albedo, fragdata.metallic);

    vec3 V = normalize(-fragdata.fragpos);

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

    vec3 light = CalcDirlight(ld, lightColor, F0, fragdata.normal, V, fragdata.albedo, fragdata.roughness, fragdata.metallic);

    // ambient  
    light += ambientScale * lightColor * fragdata.albedo;

    FragColor = vec4(light, 1.0);
}