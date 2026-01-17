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
	if (texColor.a < 0.1) discard;

	vec3 aoColor = texColor.rgb * aoValue;

	FragColor = vec4(mix(skyColor, aoColor, visibility), texColor.a);
}