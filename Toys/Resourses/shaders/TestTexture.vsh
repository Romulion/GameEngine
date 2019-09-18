#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 2) in vec2 aTex;

out vec2 TexCoords;

uniform vec3 resolution;

void main()
{
    gl_Position = vec4(aPos.x / resolution.x, aPos.y / resolution.y, 0,1.0);
    TexCoords = aTex;
}  