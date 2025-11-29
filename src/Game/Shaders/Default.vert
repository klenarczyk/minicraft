#version 330 core
layout (location = 0) in vec3 aPos; // vertex coordinates
layout (location = 1) in vec2 aTexCoord; // texture coordinates

out vec2 texCoord;

// Uniforms
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
	gl_Position = vec4(aPos, 1.0) * model * view * projection;
	texCoord = aTexCoord;
}