#version 330 core
out vec4 FragColor;
in vec2 texCoord;

uniform sampler2D u_sprite;

void main() {
	FragColor = texture(u_sprite, vec2(1.0 - texCoord.x, texCoord.y));
}