#version 330 core

/*
    cubemapSkybox fragment shader

*/

//uniform samplerCube skybox;

in vec3 texCoords;

out vec4 FragColor;


void main() {
    FragColor.rgb = (texCoords + 0.5);
    FragColor.a = 1.0;
}