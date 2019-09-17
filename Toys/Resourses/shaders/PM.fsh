#version 330 core
out vec4 FragColor;
in VS_OUT {
	vec2 Texcord;
	vec3 FragPos;
	vec3 Normal;
	vec4 lightSpace;
	vec3 NormalLocal;
} fs_in;

layout (std140) uniform light {
	vec3 LightPos;
	vec3 viewPos;
	float near_plane;
	float far_plane;
};
struct Material{
	sampler2D texture_diffuse;
	sampler2D texture_specular;
};

uniform Material material;
uniform sampler2DShadow shadowMap;

float ShadowCalculation(vec4 fragPosLightSpace)
{
	float bias = 0.005;
	vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	projCoords = projCoords * 0.5 + 0.5;
	projCoords.z -= bias;	float shadow = 1 - texture(shadowMap, projCoords);
	return shadow;
}
void main()
{
vec4 texcolor = texture(material.texture_diffuse,fs_in.Texcord);
if (texcolor.a < 0.05)
		discard;

vec4 shadowcolor  = texture(material.texture_specular,fs_in.Texcord);
FragColor = texcolor * shadowcolor;
}