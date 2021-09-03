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

void main()
{
	vec4 pos = model * vec4(aPos, 1.0);
    gl_Position =  pos;
    TexCoords = aTex;
}  