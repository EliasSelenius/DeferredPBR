#version 330 core

/*
    cubemapSkybox fragment shader

*/

//uniform samplerCube skybox;

in vec3 texCoords;

//out vec4 FragColor;
layout(location = 0) out vec4 g_Albedo_Metallic;


void main() {
    //FragColor = vec4(1.0, 2.0, 4.0, 1.0);
    g_Albedo_Metallic = vec4(1.0, 0.0, 0.0, 0.0);
}