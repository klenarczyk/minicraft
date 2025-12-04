#version 330 core
layout (location = 0) in vec3 aPos; // vertex coordinates
layout (location = 1) in vec2 aTexCoord; // texture coordinates
layout (location = 2) in float aAO; // ambient occlusion

out vec2 texCoord;
out float visibility; // Fog
out float aoValue;

// Uniforms
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

// Fog
const float density = 0.007;
const float gradient = 1.5;

void main()
{
	vec4 worldPosition = vec4(aPos, 1.0) * model;
	vec4 positionRelativeToCam = worldPosition * view;

	gl_Position = positionRelativeToCam * projection;

	texCoord = aTexCoord;
	aoValue = aAO;

	// Fog calculation
	float distance = length(positionRelativeToCam.xyz);

	visibility = exp(-pow((distance * density), gradient));
	visibility = clamp(visibility, 0.0, 1.0);
}