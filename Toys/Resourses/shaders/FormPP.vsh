#version 330 core 
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexcord;

out vec2 Texcord;

void main()
{
	gl_Position = vec4(aPos,0, 1.0);
	Texcord = vec2(aTexcord.x, 1 - aTexcord.y);
}