#version 330 core
out vec4 FragColor;

in vec2 TexCoord;
in float LightIntensity;

uniform sampler2D u_Texture;

void main() {
    vec4 texColor = texture(u_Texture, TexCoord);
    
    // If the pixel is transparent (like air or glass edges), discard it
    if(texColor.a < 0.1) discard;

    // Apply the light intensity to the texture color
    FragColor = vec4(texColor.rgb * LightIntensity, texColor.a);
}