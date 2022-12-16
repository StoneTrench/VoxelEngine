#version 330 core
out vec4 FragColor;

in vec3 in_v_worldPosition;
in vec4 in_v_color;
in vec2 in_v_TextureCoord;
in vec3 in_v_normal;
in vec4 in_fragPosLight;

uniform vec3 u_lightPosition;
uniform vec3 u_cameraPosition;
uniform sampler2D u_diffuseTex;
uniform sampler2D u_specularTex;
uniform sampler2D u_shadowMapTex;

float ambient = 0.5;
float bias = 0.00075;

vec4 CalcDirectionalLight(){
	// diffuse lighting
	vec3 normal = normalize(in_v_normal);
	vec3 lightDirection = normalize(-u_lightPosition);
	float diffuse = max(dot(normal, lightDirection), 0.0f);

	// specular lighting
	float specular = 0.0f;
	if (diffuse != 0.0f)
	{
		float specularLight = 0.50f;
		vec3 viewDirection = normalize(u_cameraPosition - in_v_worldPosition);
		vec3 halfwayVec = normalize(viewDirection + lightDirection);
		float specAmount = pow(max(dot(normal, halfwayVec), 0.0f), 16);
		specular = specAmount * specularLight;
	};

	float shadow = 0.0;
	vec3 lightCoords = in_fragPosLight.xyz / in_fragPosLight.w;
	if(lightCoords.z <= 1.0){
		vec3 lightCoordsUV = (lightCoords + 1.0) / 2.0;
		
		float closesDepth = texture(u_shadowMapTex, lightCoordsUV.xy).r;
		if (lightCoordsUV.z > closesDepth + bias)
			shadow = 1.0;
	}

	shadow = 1.0 - shadow;
	return texture(u_diffuseTex, in_v_TextureCoord) * (diffuse * shadow + ambient) + texture(u_specularTex, in_v_TextureCoord).r * specular * shadow;
}

void main() {
	FragColor = CalcDirectionalLight();
}