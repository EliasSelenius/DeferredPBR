#version 330 core

/*
    Object ID fragment shader

*/

uniform int ObjectID;

in V2F {
    vec3 fragPos;
    vec3 normal;
    vec2 uv;
} v2f;


out int objectID;
out int primitiveID;
out vec3 normal;
out vec3 fragpos;

void main() {
    objectID = ObjectID;
    primitiveID = gl_PrimitiveID;
    normal = v2f.normal;
    fragpos = v2f.fragpos;
}