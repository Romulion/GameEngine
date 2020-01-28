#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 2) in vec2 aTex;

out vec2 TexCoords;

 layout (std140) uniform system
{
    vec4 screenData;
	vec4 time;
};


uniform mat4 model;

const mat4 toScreen = mat4 (
	2, 0, 0, 0,
	0, 2, 0, 0,
	0, 0, 2, 0,
   -1,-1, 0, 1
);

void main()
{
	vec4 pos = model * vec4(aPos, 1.0);
	pos.x *= screenData.z;
	pos.y *= screenData.w;
    gl_Position =  toScreen * pos;
    TexCoords = aTex;
}  