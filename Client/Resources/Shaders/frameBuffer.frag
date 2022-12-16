#version 330 core
out vec4 FragColor;
in vec2 texCoord;

uniform sampler2D u_screenTexture;
uniform float u_gamma;

void main() {
	FragColor.rgb = pow(texture(u_screenTexture, texCoord).rgb, vec3(1.0f / u_gamma));
}