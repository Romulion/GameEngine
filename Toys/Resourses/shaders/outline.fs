#version 330 core

out vec4 FragColor;

uniform vec4 EdgeColor;

void main()
{
	FragColor = EdgeColor;
}