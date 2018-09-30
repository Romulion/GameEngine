#version 330 core 
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexcord;


uniform mat4 model;
uniform mat4 pvm;
uniform mat4 NormalMat;
uniform mat4 lightSpacePos;

out VS_OUT {
	vec2 Texcord;
	vec3 FragPos;
	vec3 Normal;
	vec4 lightSpace;
} vs_out;

void main()
{

	gl_Position =  pvm  * vec4(aPos, 1.0);
	
	vs_out.Texcord = aTexcord;	
	vs_out.FragPos = vec3(model * vec4(aPos, 1.0));
	vs_out.Normal = mat3(NormalMat)  * aNormal;
	vs_out.lightSpace = lightSpacePos * vec4(vs_out.FragPos,1.0);
	//Normal = mat3(NormalMat) * aNormal;
	
}