#version 330 core
out vec4 FragColor;
in VS_OUT {
	vec4 Texcord;
	vec3 FragPos;
	vec3 Normal;
	vec4 lightSpace;
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

void main()
{
	vec4 texcolor = texture(material.texture_diffuse,fs_in.Texcord.xy);
	if (texcolor.a < 0.05)
		discard;
	vec4 shadowcolor = texture(material.texture_specular,fs_in.Texcord.zw) * 0.6 + vec4(0.4);
	shadowcolor.w = 1;
	FragColor = texcolor * shadowcolor;
}