#version 330 core
layout (location = 0) in vec2 inPos;
layout (location = 1) in vec2 inTexCoords;

out vec2 texCoord;

uniform mat4 u_model;

void main(){
	gl_Position = vec4(inPos.xy, 0.0, 1.0) * u_model;
	texCoord = inTexCoords;
}