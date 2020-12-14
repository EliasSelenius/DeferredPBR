#version 330 core



/*
    text fragment shader

*/

uniform sampler2D atlas;
uniform vec4 color;

in V2F {
    vec2 uv;
} v2f;

out vec4 FragColor;

void main() {
    FragColor = texture(atlas, v2f.uv) * color; 
}