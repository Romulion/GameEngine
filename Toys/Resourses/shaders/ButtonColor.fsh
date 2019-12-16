#version 330 core
in vec2 TexCoords;
out vec4 color;

uniform vec3 col;

void main()
{
    color = vec4(col,1);
}