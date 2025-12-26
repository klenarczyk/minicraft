#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aUv;

out vec2 TexCoords;

uniform mat4 u_Projection;
uniform mat4 u_Model;
// (MinU, MinV, WidthU, HeightV)
uniform vec4 u_UvTransform; 

void main()
{
    // Transform base UVs (0..1) to Atlas UVs
    TexCoords = vec2(
        u_UvTransform.x + (aUv.x * u_UvTransform.z),
        u_UvTransform.y + (aUv.y * u_UvTransform.w)
    );

    gl_Position = u_Projection * u_Model * vec4(aPos, 0.0, 1.0);
}