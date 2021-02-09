#version 330 core

/*
    cubemapSkybox fragment shader

*/

uniform samplerCube skybox;

in vec3 texCoords;
out vec4 FragColor;


void main() {
    vec3 dir = normalize(texCoords);
    FragColor = texture(skybox, dir);
}