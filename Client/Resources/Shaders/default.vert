#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aColor;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec2 aTex;

out vec3 in_v_worldPosition;
out vec4 in_v_color;
out vec2 in_v_TextureCoord;
out vec3 in_v_normal;
out vec4 in_fragPosLight;

uniform mat4 u_cameraMat;
uniform mat4 u_modelMat;
uniform mat4 u_lightProjectionMat;

void main()
{
	vec4 crntPos4 = u_modelMat * vec4(aPosition, 1.0);
    in_v_worldPosition = vec3(crntPos4.xyz);
    in_v_TextureCoord = aTex;
    in_v_normal = aNormal;
    in_v_color = vec4(aColor.rgb, 1.0);
	in_fragPosLight = u_lightProjectionMat * crntPos4;
    gl_Position = u_cameraMat * crntPos4;
}