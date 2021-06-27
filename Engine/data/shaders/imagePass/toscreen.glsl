#version 330 core

uniform sampler2D colorInput;
uniform sampler2D brightnessInput;

in V2F {
    vec2 uv;
} v2f;

out vec4 FragColor;

void main() {

    vec3 color = texture(colorInput, v2f.uv).rgb;


    FragColor = vec4(color, 1.0);

}