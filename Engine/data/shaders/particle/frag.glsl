#version 330 core

out vec4 FragColor;

in V2F {
    vec4 color;
} v2f;


void main() {
    FragColor = v2f.color;
}