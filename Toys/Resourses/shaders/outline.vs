#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;

layout (std140) uniform skeleton
{
    mat4 gBones[500];
};

uniform mat4 pvm;
uniform float outline_scale;
uniform float EdgeScaler;

void main()
{

	
	vec3 outline =  aPos + aNormal * outline_scale * EdgeScaler;
    gl_Position =  pvm  * vec4(outline, 1.0);
}