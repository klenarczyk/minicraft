#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

uniform mat4 model;
uniform mat4 projection;

uniform vec4 uvTransform;

void main()
{
    vec4 world = model * vec4(aPos, 0.0, 1.0);
    gl_Position = projection * world;
    TexCoord = (aTexCoord * uvTransform.zw) + uvTransform.xy;
}