#version 330 core
out vec4 FragColor;

in vec2 TexCoord;
in float LightIntensity;

uniform sampler2D u_Texture;

void main() {
    vec4 texColor = texture(u_Texture, TexCoord);
    
    // If transparent, discard
    if(texColor.a < 0.1) discard;

    // Apply the light intensity
    FragColor = vec4(texColor.rgb * LightIntensity, texColor.a);
}