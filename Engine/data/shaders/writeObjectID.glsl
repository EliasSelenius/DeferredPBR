#version 330 core

/*
    Object ID fragment shader

*/

uniform int ObjectID = 36;

out int outputID;

void main() {
    outputID = ObjectID;
}