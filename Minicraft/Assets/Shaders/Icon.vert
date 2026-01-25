#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;

out vec2 TexCoord;
out float LightIntensity;

uniform mat4 u_Mvp;

void main() {
    gl_Position = u_Mvp * vec4(aPos, 1.0);
    TexCoord = aTexCoord;

    // Standard "Top-Down-Left" light direction
    vec3 lightDir = normalize(vec3(-0.5, 1.0, 0.5));
    
    // Lambertian shading
    float diff = max(dot(aNormal, lightDir), 0.0);
    
    // Ambient light
    LightIntensity = diff * 0.7 + 0.3; 
}