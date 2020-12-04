#version 330 core



/*
    text fragment shader

*/

uniform sampler2D atlas;
uniform vec3 color;

in V2F {
    vec2 uv;
} v2f;

out vec4 FragColor;

void main() {
    FragColor = vec4(texture(atlas, v2f.uv).rgb * color, 1.0); 
}