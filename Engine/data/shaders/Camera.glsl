
#pragma UBO Camera bind

layout (std140) uniform Camera {
    mat4 view;
    mat4 projection;
} camera;