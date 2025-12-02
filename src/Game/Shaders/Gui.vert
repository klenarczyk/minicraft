#version 330 core
layout (location = 0) in vec2 aPosition;

uniform float aspectRatio; 
uniform float scale;

void main()
{
    // Adjust by 1 / aspectRation to maintain the squareness
    gl_Position = vec4(aPosition.x / aspectRatio * scale, aPosition.y * scale, 0.0, 1.0);
}