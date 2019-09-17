#version 330 core
layout (location = 0) in vec3 aPos;


layout (std140) uniform space {
	mat4 model;
	mat4 pvm;
	mat4 NormalMat;
	mat4 lightSpacePos;
	mat4 pv;
};

uniform mat4 world;

void main()
{
    gl_Position = pv * world * vec4(aPos, 1.0);
}