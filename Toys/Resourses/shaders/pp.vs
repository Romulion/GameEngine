#version 330 core 
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexcord;

out vec2 Texcord;

void main()
{
	gl_Position = vec4(aPos, 1.0);
	Texcord = aTexcord;
	
}