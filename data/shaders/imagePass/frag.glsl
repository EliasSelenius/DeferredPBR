#version 330 core

/*
    imagePass fragment shader

*/

uniform sampler2D input;

in V2F {
    vec2 uv;
} v2f;

out vec4 FragColor;

void main() {

    vec3 color = texture(input, v2f.uv).rgb;

    // reinhard tone mapping
    color = color / (color + vec3(1.0));

    // gamma correction
    color = pow(color, vec3(1.0 / 2.2));

    FragColor = vec4(color, 1.0);

}