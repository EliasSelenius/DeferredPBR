#version 330 core

/*
    imagePass fragment shader

*/

uniform sampler2D tex_input;

in V2F {
    vec2 uv;
} v2f;

out vec4 FragColor;

void main() {

    vec3 color = texture(tex_input, v2f.uv).rgb;

    // reinhard tone mapping
    //color = color / (color + vec3(1.0));
    // alternative tone mapping with exposure:
    float exposure = 1.0;
    color = vec3(1.0) - exp(-color * exposure);


    // gamma correction
    color = pow(color, vec3(1.0 / 2.2));

    FragColor = vec4(color, 1.0);

}