#version 330 core
in vec2 TexCoord;

out vec4 FragColor;

uniform sampler2D guiTexture;
uniform vec3 color;

void main()
{
    vec4 texColor = texture(guiTexture, TexCoord);

    if (texColor.a < 0.1) discard;

    FragColor = texColor * vec4(color, 1); 
}