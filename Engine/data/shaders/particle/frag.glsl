#version 330 core

uniform sampler2D tex; 

out vec4 FragColor;

in V2F {
    vec4 color;
} v2f;


void main() {
    FragColor = texture(tex, gl_PointCoord) * v2f.color;
}