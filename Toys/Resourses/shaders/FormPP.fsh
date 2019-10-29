#version 330 core

out vec4 FragColor;

in vec2 Texcord;
uniform sampler2D texture_diffuse;
uniform vec3 colorOffset;

void main()
{
    vec4 rValue = texture2D(texture_diffuse, vec2(Texcord.s - colorOffset.r,Texcord.t));  
    vec4 gValue = texture2D(texture_diffuse, vec2(Texcord.s - colorOffset.g,Texcord.t));
    vec4 bValue = texture2D(texture_diffuse, vec2(Texcord.s - colorOffset.b,Texcord.t));
	vec4 Value = texture2D(texture_diffuse, vec2(Texcord));
    FragColor = vec4(rValue.r, gValue.g, bValue.b, Value.w);
} 