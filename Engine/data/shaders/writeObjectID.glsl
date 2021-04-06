#version 330 core

/*
    Object ID fragment shader

*/

uniform int ObjectID;

out int outputID;

void main() {
    outputID = ObjectID;
}