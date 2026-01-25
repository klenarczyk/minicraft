#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float aAO;

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
	vec4 worldPosition = model * vec4(aPos, 1.0);
	vec4 positionRelativeToCam = view * worldPosition;

	gl_Position = projection * positionRelativeToCam;

	texCoord = aTexCoord;
	aoValue = aAO;

	// Fog calculation
	float distance = length(positionRelativeToCam.xyz);

	visibility = exp(-pow((distance * density), gradient));
	visibility = clamp(visibility, 0.0, 1.0);
}