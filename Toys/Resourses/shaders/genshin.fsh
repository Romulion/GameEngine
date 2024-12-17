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
        sampler2D texture_toon;
        sampler2D texture_spere;
        sampler2D texture_extra;
        vec4 diffuse_color;
        vec3 specular_color;
        float specular_power;
        vec3 ambient_color;
};
uniform Material material;
uniform sampler2DShadow shadowMap;
void main()
{
const vec3 amb = vec3(0.5);
vec4 texcolor = texture(material.texture_diffuse,fs_in.Texcord);
texcolor.a = 1;

vec3 normal = normalize(fs_in.Normal);
vec3 lightDir = normalize(LightPos - fs_in.FragPos);
float diffuse = 1 - max(dot(lightDir,normal) , 0.02);
vec3 shadowcolor  = vec3(diffuse * 0.2 + 0.8);
FragColor = (vec4(clamp(amb+ material.diffuse_color.xyz * 0.5 + material.ambient_color,0.0,1.0),1) * (texcolor) * vec4(shadowcolor,1)) ;
}