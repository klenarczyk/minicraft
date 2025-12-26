#version 330 core
in vec2 texCoord;
in float visibility;
in float aoValue;

out vec4 FragColor;

uniform sampler2D texture0;
uniform vec3 skyColor;

void main() 
{
	vec4 texColor = texture(texture0, texCoord);

	vec3 aoColor = texColor.rgb * aoValue;

	//FragColor = vec4(aoValue, aoValue, aoValue, 1.0); // Test for Ambient Occlusion
	FragColor = vec4(mix(skyColor, aoColor, visibility), texColor.a);
	//FragColor = vec4(texCoord.x, texCoord.y, 0.0, 1.0); // Debug fo verts

	if (FragColor.a < 0.1) discard;
}