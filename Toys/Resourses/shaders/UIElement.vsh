#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 2) in vec2 aTex;

out vec2 TexCoords;

uniform mat4 model;

const mat4 toScreen = mat4 (
	2, 0, 0, 0,
	0, 2, 0, 0,
	0, 0, 2, 0,
   -1,-1, 0, 1
);

void main()
{
    gl_Position =  toScreen * model * vec4(aPos,1.0);
    TexCoords = aTex;
}  