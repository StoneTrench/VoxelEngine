#version 330 core
layout (location = 0) in vec3 aPosition;

uniform mat4 u_modelMat;
uniform mat4 u_lightProjectionMat;

void main()
{
    vec4 crntPos4 = u_modelMat * vec4(aPosition, 1.0f);
    gl_Position = u_lightProjectionMat * crntPos4;
}