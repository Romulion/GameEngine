#version 330 core
in vec2 TexCoords;
out vec4 color;

uniform vec3 color_mask;

struct Material{
	sampler2D texture_diffuse;
};

uniform Material material;

void main()
{
    color = texture(material.texture_diffuse, TexCoords) * vec4(color_mask,1);
}