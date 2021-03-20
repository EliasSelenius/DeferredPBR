#version 330 core



/*
    text fragment shader

*/

uniform sampler2D backgroundImage;

in V2F {
    vec2 uv;
    vec4 color;
} v2f;

out vec4 FragColor;

void main() {
    FragColor = texture(backgroundImage, v2f.uv) * v2f.color; 
}